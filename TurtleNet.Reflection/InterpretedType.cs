using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace TurtleNet.Reflection
{
	sealed class InterpretedType : Type
	{
		internal InterpretedType(string name, string @namespace, Assembly assembly, Type[] genericTypeArguments)
		{
			Assembly = assembly;
			Name = name;
			Namespace = @namespace;
			FullName = string.IsNullOrEmpty(@namespace) ? name : $"{@namespace}.{name}";
			GenericTypeArguments = genericTypeArguments;
		}

		public override string Name { get; }
		public override string Namespace { get; }
		Type? _baseType;
		public override Type? BaseType => _baseType;

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			throw new NotImplementedException();
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException();
		}

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type GetElementType()
		{
			throw new NotImplementedException();
		}

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type GetInterface(string name, bool ignoreCase)
		{
			throw new NotImplementedException();
		}

		public override Type[] GetInterfaces()
		{
			throw new NotImplementedException();
		}

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
		  => GetMethodsInt(bindingAttr).Where(mi => string.Equals(mi.Name, name, StringComparison.Ordinal)).ToArray();

		protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers)
		  => GetMethodsInt(bindingAttr).FirstOrDefault(mi => string.Equals(mi.Name, name, StringComparison.Ordinal) && (types == null || mi.GetParameters().Select(x => x.ParameterType).SequenceEqual(types)));

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		  => GetMethodsInt(bindingAttr).ToArray();

		IEnumerable<MethodInfo> GetMethodsInt(BindingFlags bindingAttr)
		  => _allMethods.OfType<InterpretedMethod>();

		public override Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException();
		}

		protected override bool HasElementTypeImpl()
		{
			throw new NotImplementedException();
		}

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
		{
			throw new NotImplementedException();
		}

		protected override bool IsArrayImpl()
		{
			throw new NotImplementedException();
		}

		protected override bool IsByRefImpl()
		{
			throw new NotImplementedException();
		}

		protected override bool IsCOMObjectImpl()
		{
			throw new NotImplementedException();
		}

		protected override bool IsPointerImpl()
		{
			throw new NotImplementedException();
		}

		protected override bool IsPrimitiveImpl()
		{
			throw new NotImplementedException();
		}

		public override Assembly Assembly { get; }
		public override string AssemblyQualifiedName { get; }
		public override string FullName { get; }
		public override Guid GUID { get; }
		public override Module Module { get; }
		public override Type UnderlyingSystemType { get; }

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override Type[] GenericTypeArguments { get; }
		public override bool ContainsGenericParameters => GenericTypeArguments.Length != 0;
		public override Type[] GetGenericArguments() => GenericTypeArguments;
		public override bool IsGenericTypeDefinition => ContainsGenericParameters;
		public override bool IsGenericType => ContainsGenericParameters;
		public override bool IsConstructedGenericType => false;

		public override Type MakeGenericType(params Type[] typeArguments)
		{
			throw new Exception();
		}

		internal void SetBaseType(Type? type)
		  => _baseType = type;

		IInterpretedMethodBase[] _allMethods;
		internal IInterpretedMethodBase[] AllMethods => _allMethods;
		internal void SetAllMethodBases(IInterpretedMethodBase[] methods)
		  => _allMethods = methods;
	}
}
