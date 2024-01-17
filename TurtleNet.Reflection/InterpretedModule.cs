using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using TurtleNet.Runtime.Internal;

namespace TurtleNet.Reflection
{
	//https://www.demo2s.com/csharp/csharp-metadatareader-tutorial-with-examples.html
	//https://www.demo2s.com/csharp/csharp-methodspecification-tutorial-with-examples.html
	sealed class InterpretedModule : Module
	{
		readonly PEReader _peReader;
		readonly MetadataReader _mdReader;
		readonly InterpretedAssembly _assembly;
		readonly Dictionary<int, (TypeDefinitionHandle Handle, TypeDefinition Definition, InterpretedType Type)> _typeinfos;
		readonly Dictionary<EntityHandle, Assembly> _asmCache;
		readonly Dictionary<EntityHandle, Type> _typeCache;
		readonly Dictionary<MethodDefinitionHandle, IInterpretedMethodBase> _methodCache;
		readonly InterpretedType[] _types;

		internal InterpretedModule(PEReader peReader, InterpretedAssembly assembly)
		{
			_peReader = peReader;
			_mdReader = peReader.GetMetadataReader();

			_assembly = assembly;

			_nameTable = new Dictionary<StringHandle, string>(64);
			_nameTableUser = new Dictionary<UserStringHandle, string>(64);
			_asmCache = new Dictionary<EntityHandle, Assembly>(_mdReader.GetTableRowCount(TableIndex.AssemblyRef));
			_typeCache = new Dictionary<EntityHandle, Type>(_mdReader.GetTableRowCount(TableIndex.TypeDef));
			_methodCache = new Dictionary<MethodDefinitionHandle, IInterpretedMethodBase>(_mdReader.GetTableRowCount(TableIndex.MethodDef));

			_typeinfos = _mdReader.TypeDefinitions.ToDictionary(x => MetadataTokens.GetToken(x), CreateTypeInfo);

			//get entry point method handle
			MethodDefinitionHandle entryPointHandle = default;
			CorHeader corHeader = peReader.PEHeaders.CorHeader!;
			if ((corHeader.Flags & CorFlags.NativeEntryPoint) == 0)
			{
				int entryPointToken = corHeader.EntryPointTokenOrRelativeVirtualAddress;
				if (entryPointToken != 0)
				{
					EntityHandle handle = MetadataTokens.EntityHandle(entryPointToken);
					if (handle.Kind == HandleKind.MethodDefinition)
						entryPointHandle = (MethodDefinitionHandle)handle;
				}
			}

			foreach (var item in _typeinfos)
			{
				InterpretedType type = item.Value.Type;
				type.SetBaseType(ResolveType(item.Value.Handle, Type.EmptyTypes));
				type.SetAllMethodBases(item.Value.Definition.GetMethods().Select(x => ResolveMethod(x, item.Value.Type)).ToArray());
			}

			foreach (var item in _typeinfos)
			{
				InterpretedType type = item.Value.Type;
				foreach (IInterpretedMethodBase im in type.AllMethods)
					im.SetBody(CreateMethodBody(_peReader.GetMethodBody(im.Address), type, ((MethodBase)im).GetGenericArguments()));
			}

			if (entryPointHandle != default)
				EntryPoint = (MethodInfo)_methodCache[entryPointHandle];

			_nameTable.Clear();
			_nameTableUser.Clear();
			_asmCache.Clear();
			_typeCache.Clear();
			_methodCache.Clear();

			_types = _typeinfos.Select(x => x.Value.Type).ToArray();
		}

		#region loader
		(TypeDefinitionHandle, TypeDefinition, InterpretedType) CreateTypeInfo(TypeDefinitionHandle handle)
		{
			TypeDefinition td = _mdReader.GetTypeDefinition(handle);
			InterpretedTypeGenericParm[] genericParms = td.GetGenericParameters().Select(gph =>
			{
				GenericParameter gp = _mdReader.GetGenericParameter(gph);
				return new InterpretedTypeGenericParm(gp.Index, GetString(gp.Name), gp.Attributes);
			}).ToArray();
			InterpretedType result = new InterpretedType(GetString(td.Name), GetString(td.Namespace), _assembly, genericParms);
			foreach (InterpretedTypeGenericParm gp in genericParms)
				gp.SetDeclaringType(result);
			return (handle, td, result);
		}

		readonly Dictionary<StringHandle, string> _nameTable;
		string GetString(StringHandle handle)
		  => _nameTable.TryGetValue(handle, out string? result)
			 ? result
			 : _nameTable[handle] = _mdReader.GetString(handle);
		readonly Dictionary<UserStringHandle, string> _nameTableUser;
		string GetUserString(UserStringHandle handle)
		  => _nameTableUser.TryGetValue(handle, out string? result)
			 ? result
			 : _nameTableUser[handle] = _mdReader.GetUserString(handle);

		Type? ResolveType(EntityHandle handle, Type[]? genericTypeArguments)
		{
			if (handle.IsNil)
				return null;

			if (_typeCache.TryGetValue(handle, out Type? result2))
				return result2;

			Type result;
			if (handle.Kind == HandleKind.TypeDefinition)
				result = _typeinfos.First(x => x.Value.Handle == handle).Value.Type;
			else if (handle.Kind == HandleKind.TypeSpecification)
			{
				TypeSpecification ts = _mdReader.GetTypeSpecification((TypeSpecificationHandle)handle);
				result = ts.DecodeSignature(new SignatureTypeProvider(_typeinfos), new SignatureTypeProvider.Context(genericTypeArguments, Type.EmptyTypes));
			}
			else if (handle.Kind == HandleKind.TypeReference)
			{
				TypeReference tr = _mdReader.GetTypeReference((TypeReferenceHandle)handle);
				string name = _mdReader.GetString(tr.Name);
				string ns = _mdReader.GetString(tr.Namespace);
				EntityHandle eh = tr.ResolutionScope;
				if (eh.IsNil) //Nil pořídí, zda cílový typ musí být vyřešen hledáním ExportedTypes odpovídajícího Namespace a Name .
				{
					throw new NotImplementedException();
				}
				else if (eh.Kind == HandleKind.TypeReference) //TypeReferenceHandle nadřazeného typu, je-li cílový typ vnořený typ.
				{
					throw new NotImplementedException();
				}
				else if (eh.Kind == HandleKind.ModuleReference) //ModuleReferenceHandle, pokud je cílový typ definován v jiném modulu v rámci stejného sestavení jako tento.
				{
					throw new NotImplementedException();
				}
				else if (eh.Kind == HandleKind.ModuleDefinition) //ModuleDefinition, pokud je cílový typ definován v aktuálním modulu. To by nemělo být v modulu CLI komprimovaných metadat.
				{
					throw new NotImplementedException();
				}
				else if (eh.Kind == HandleKind.AssemblyReference) //AssemblyReferenceHandle, pokud je cílový typ definován v jiném sestavení než aktuální modul.
					return ResolveAssembly((AssemblyReferenceHandle)eh).GetType($"{ns}.{name}");
				else
					throw new NotSupportedException();
			}
			else
				throw new NotSupportedException();

			_typeCache.Add(handle, result);
			return result;
		}

		Assembly? ResolveAssembly(AssemblyReferenceHandle handle)
		{
			if (handle.IsNil)
				return null;

			if (_asmCache.TryGetValue(handle, out Assembly? result2))
				return result2;

			Assembly result;
			AssemblyReference ar = _mdReader.GetAssemblyReference(handle);
			result = InterpretedAppDomain.CurrentDomain.Load(ar.GetAssemblyName());

			_asmCache.Add(handle, result);
			return result;
		}

		IInterpretedMethodBase ResolveMethod(MethodDefinitionHandle handle, Type declaringType)
		{
			MethodDefinition md = _mdReader.GetMethodDefinition(handle);
			InterpretedMethodGenericParm[] genericParms = md.GetGenericParameters().Select(gph =>
			{
				var gp = _mdReader.GetGenericParameter(gph);
				return new InterpretedMethodGenericParm(gp.Index, GetString(gp.Name), gp.Attributes);
			}).ToArray();
			MethodSignature<Type> ms = md.DecodeSignature(new SignatureTypeProvider(_typeinfos), new SignatureTypeProvider.Context(declaringType.GenericTypeArguments, genericParms));

			TypeDefinitionHandle declaringTypeHandle = md.GetDeclaringType();

			ParameterHandleCollection parmameterHandles = md.GetParameters();
			ParameterInfo[] parms = new ParameterInfo[parmameterHandles.Count];
			int parmIndex = 0;
			foreach (ParameterHandle parameterHandle in parmameterHandles)
			{
				Parameter parameter = _mdReader.GetParameter(parameterHandle);
				InterpretedParameter parm = new InterpretedParameter(GetString(parameter.Name), ms.ParameterTypes[parmIndex]);
				parms[parmIndex++] = parm;
			}

			var result = new InterpretedMethod(md.RelativeVirtualAddress, GetString(md.Name), declaringType, genericParms, ms.ReturnType, parms);
			foreach (InterpretedMethodGenericParm gp in genericParms)
				gp.SetDeclaringMethod(result);
			_methodCache.Add(handle, result);
			return result;
		}

		InterpretedMethodBody CreateMethodBody(MethodBodyBlock bodyBlock, Type declaringType, Type[] genericArguments)
		{
			IlInstruction[] instructions = new IlReader().ReadMethod(bodyBlock.GetILBytes()!, declaringType, genericArguments, this);
			Type[]? locals = bodyBlock.LocalSignature.IsNil ? null : _mdReader.GetStandaloneSignature(bodyBlock.LocalSignature).DecodeLocalSignature(new SignatureTypeProvider(_typeinfos), new SignatureTypeProvider.Context(declaringType.GenericTypeArguments, genericArguments)).ToArray();
			return new InterpretedMethodBody(instructions, bodyBlock.MaxStack, locals);
		}

		class SignatureTypeProvider : ISignatureTypeProvider<Type, SignatureTypeProvider.Context>
		{
			readonly Dictionary<int, (TypeDefinitionHandle Handle, TypeDefinition Definition, InterpretedType Type)> _typeinfos;
			internal SignatureTypeProvider(Dictionary<int, (TypeDefinitionHandle Handle, TypeDefinition Definition, InterpretedType Type)> typeinfos)
			{
				_typeinfos = typeinfos;
			}

			public Type GetFunctionPointerType(MethodSignature<Type> signature)
			{
				throw new NotImplementedException();
			}

			public Type GetGenericMethodParameter(Context genericContext, int index)
			  => genericContext.GenericMethodArguments[index];

			public Type GetGenericTypeParameter(Context genericContext, int index)
			  => genericContext.GenericTypeArguments[index];

			public Type GetModifiedType(Type modifier, Type unmodifiedType, bool isRequired)
			{
				throw new NotImplementedException();
			}

			public Type GetPinnedType(Type elementType)
			{
				throw new NotImplementedException();
			}

			public Type GetTypeFromSpecification(MetadataReader reader, Context genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
			{
				throw new NotImplementedException();
			}

			public Type GetArrayType(Type elementType, ArrayShape shape)
			{
				throw new NotImplementedException();
			}

			public Type GetByReferenceType(Type elementType)
			{
				throw new NotImplementedException();
			}

			public Type GetGenericInstantiation(Type genericType, ImmutableArray<Type> typeArguments)
			  //=> genericType.MakeGenericType(typeArguments.ToArray());
			  => genericType;

			public Type GetPointerType(Type elementType)
			{
				throw new NotImplementedException();
			}

			public Type GetSZArrayType(Type elementType)
			{
				throw new NotImplementedException();
			}

			public Type GetPrimitiveType(PrimitiveTypeCode typeCode)
			  => typeCode switch
			  {
				  PrimitiveTypeCode.Void => typeof(void),
				  PrimitiveTypeCode.Boolean => typeof(bool),
				  PrimitiveTypeCode.Char => typeof(char),
				  PrimitiveTypeCode.SByte => typeof(sbyte),
				  PrimitiveTypeCode.Byte => typeof(byte),
				  PrimitiveTypeCode.Int16 => typeof(short),
				  PrimitiveTypeCode.UInt16 => typeof(ushort),
				  PrimitiveTypeCode.Int32 => typeof(int),
				  PrimitiveTypeCode.UInt32 => typeof(uint),
				  PrimitiveTypeCode.Int64 => typeof(long),
				  PrimitiveTypeCode.UInt64 => typeof(ulong),
				  PrimitiveTypeCode.Single => typeof(float),
				  PrimitiveTypeCode.Double => typeof(double),
				  PrimitiveTypeCode.String => typeof(string),
				  //PrimitiveTypeCode.TypedReference => typeof(),
				  PrimitiveTypeCode.IntPtr => typeof(IntPtr),
				  PrimitiveTypeCode.UIntPtr => typeof(UIntPtr),
				  PrimitiveTypeCode.Object => typeof(object),
				  _ => throw new NotImplementedException()
			  };

			public Type GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
			  => _typeinfos[MetadataTokens.GetToken(handle)].Type;

			public Type GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
			{
				throw new NotImplementedException();
			}

			public class Context
			{
				public Context(Type[]? genericTypeArguments, Type[]? genericMethodArguments)
				{
					GenericTypeArguments = genericTypeArguments;
					GenericMethodArguments = genericMethodArguments;
				}

				public Type[]? GenericTypeArguments;
				public Type[]? GenericMethodArguments;
			}
		}
		#endregion loader

		public override Type[] GetTypes()
		  => _types;

		public override string ResolveString(int metadataToken)
		  => GetUserString(MetadataTokens.UserStringHandle(metadataToken));

		public override MethodBase? ResolveMethod(int metadataToken, Type[]? genericTypeArguments, Type[]? genericMethodArguments)
		{
			MethodBase? ResolveMemberReference(MemberReferenceHandle mrh, Type[] genericMethodArguments)
			{
				MemberReference mr = _mdReader.GetMemberReference(mrh);
				string name = GetString(mr.Name);
				Type declType = ResolveType(mr.Parent, genericTypeArguments)!;
				MemberInfo[] mis = declType.GetMember(name);
				if (mis.Length == 0)
					return null;

				MethodSignature<Type> ms = mr.DecodeMethodSignature(new SignatureTypeProvider(_typeinfos), new SignatureTypeProvider.Context(declType.GenericTypeArguments, genericMethodArguments));
				return mis.OfType<MethodBase>()
				  .First(x =>
				  {
					  IEnumerator en1 = x.GetParameters().GetEnumerator();
					  ImmutableArray<Type>.Enumerator en2 = ms.ParameterTypes.GetEnumerator();
					  while (true)
					  {
						  bool b1 = en1.MoveNext();
						  bool b2 = en2.MoveNext();
						  if (b1 != b2)
							  return false;
						  if (!b1)
							  return true;

						  Type t1 = ((ParameterInfo)en1.Current).ParameterType;
						  Type t2 = en2.Current;
						  if (t1.IsGenericParameter)
							  continue;
						  if (!t1.IsAssignableFrom(t2))
							  return false;
					  }
				  });
			}

			EntityHandle eh = MetadataTokens.EntityHandle(metadataToken);
			if (eh.Kind == HandleKind.MethodDefinition)
			{
				int addr = _mdReader.GetMethodDefinition((MethodDefinitionHandle)eh).RelativeVirtualAddress;
				return (MethodBase)_types.SelectMany(x => x.AllMethods).First(x => x.Address == addr);
			}
			else if (eh.Kind == HandleKind.MemberReference)
				return ResolveMemberReference((MemberReferenceHandle)eh, null);
			else if (eh.Kind == HandleKind.MethodSpecification)
			{
				MethodSpecification ms = _mdReader.GetMethodSpecification((MethodSpecificationHandle)eh);
				if (ms.Method.Kind == HandleKind.MemberReference)
					return ResolveMemberReference((MemberReferenceHandle)ms.Method, ms.DecodeSignature(new SignatureTypeProvider(_typeinfos), new SignatureTypeProvider.Context(genericTypeArguments, genericMethodArguments)).ToArray());
				throw new NotImplementedException();
			}
			else
				throw new NotImplementedException();
		}

		public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			EntityHandle eh = MetadataTokens.EntityHandle(metadataToken);
			if (eh.Kind == HandleKind.TypeSpecification)
				return _mdReader.GetTypeSpecification((TypeSpecificationHandle)eh).DecodeSignature(new SignatureTypeProvider(_typeinfos), new SignatureTypeProvider.Context(genericTypeArguments, genericMethodArguments));
			else
				throw new NotImplementedException();
		}

		internal MethodInfo EntryPoint { get; }
	}
}
