using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace TurtleNet.Reflection
{
	public sealed class InterpretedAssembly : Assembly
	{
		//All loaded assemblies

		public static new Assembly Load(byte[] rawAssembly)
		  => new InterpretedAssembly(rawAssembly/*, null*/);

		//public static Assembly Load(Stream rawAssembly, Stream rawSymbolStore)
		//  => new InterpretedAssembly(rawAssembly, rawSymbolStore);

		private InterpretedAssembly(byte[] rawAssembly/*, Stream rawSymbolStore*/)
		{
			if (rawAssembly == null)
				throw new ArgumentNullException(nameof(rawAssembly));

			using (PEReader peReader = new PEReader(rawAssembly.ToImmutableArray()))
				ManifestModule = new InterpretedModule(peReader, this);
		}

		//// Summary:
		////     Gets a collection that contains this assembly's custom attributes.
		////
		//// Returns:
		////     A collection that contains this assembly's custom attributes.
		//public override IEnumerable<CustomAttributeData> CustomAttributes { get; }
		////
		//// Summary:
		////     Gets a collection of the types defined in this assembly.
		////
		//// Returns:
		////     A collection of the types defined in this assembly.
		//public override IEnumerable<TypeInfo> DefinedTypes { get; }

		public override MethodInfo? EntryPoint => ((InterpretedModule)ManifestModule).EntryPoint;

		////
		//// Summary:
		////     Gets the URI, including escape characters, that represents the codebase.
		////
		//// Returns:
		////     A URI with escape characters.
		//[Obsolete("Assembly.CodeBase and Assembly.EscapedCodeBase are only included for .NET Framework compatibility. Use Assembly.Location instead.", DiagnosticId = "SYSLIB0012", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
		//public override string EscapedCodeBase { get; }
		////
		//// Summary:
		////     Gets a collection of the public types defined in this assembly that are visible
		////     outside the assembly.
		////
		//// Returns:
		////     A collection of the public types defined in this assembly that are visible outside
		////     the assembly.
		//public override IEnumerable<Type> ExportedTypes { get; }
		////
		//// Summary:
		////     Gets the display name of the assembly.
		////
		//// Returns:
		////     The display name of the assembly.
		//public override string? FullName { get; }
		////
		//// Summary:
		////     Gets a value indicating whether the assembly was loaded from the global assembly
		////     cache.
		////
		//// Returns:
		////     true if the assembly was loaded from the global assembly cache; otherwise, false.
		//[Obsolete("The Global Assembly Cache is not supported.", DiagnosticId = "SYSLIB0005", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
		//public override bool GlobalAssemblyCache { get; }
		////
		//// Summary:
		////     Gets the location of the assembly as specified originally, for example, in an
		////     System.Reflection.AssemblyName object.
		////
		//// Returns:
		////     The location of the assembly as specified originally.
		//[Obsolete("Assembly.CodeBase and Assembly.EscapedCodeBase are only included for .NET Framework compatibility. Use Assembly.Location instead.", DiagnosticId = "SYSLIB0012", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
		//public override string? CodeBase { get; }
		////
		//// Summary:
		////     Gets the host context with which the assembly was loaded.
		////
		//// Returns:
		////     An System.Int64 value that indicates the host context with which the assembly
		////     was loaded, if any.
		//public override long HostContext { get; }
		////
		//// Summary:
		////     Gets a value that indicates whether this assembly is held in a collectible System.Runtime.Loader.AssemblyLoadContext.
		////
		//// Returns:
		////     true if this assembly is held in a collectible System.Runtime.Loader.AssemblyLoadContext;
		////     otherwise, false.
		//public override bool IsCollectible { get; }
		////
		//// Summary:
		////     Gets a value that indicates whether the current assembly was generated dynamically
		////     in the current process by using reflection emit.
		////
		//// Returns:
		////     true if the current assembly was generated dynamically in the current process;
		////     otherwise, false.
		//public override bool IsDynamic { get; }
		////
		//// Summary:
		////     Gets a value that indicates whether the current assembly is loaded with full
		////     trust.
		////
		//// Returns:
		////     true if the current assembly is loaded with full trust; otherwise, false.
		//public bool IsFullyTrusted { get; }
		////
		//// Summary:
		////     Gets the full path or UNC location of the loaded file that contains the manifest.
		////
		//// Returns:
		////     The location of the loaded file that contains the manifest. If the loaded file
		////     was shadow-copied, the location is that of the file after being shadow-copied.
		////     If the assembly is loaded from a byte array, such as when using the System.Reflection.Assembly.Load(System.Byte[])
		////     method overload, the value returned is an empty string ("").
		////
		//// Exceptions:
		////   T:System.NotSupportedException:
		////     The current assembly is a dynamic assembly, represented by an System.Reflection.Emit.AssemblyBuilder
		////     object.
		//public override string Location { get; }
		////
		//// Summary:
		////     Gets the module that contains the manifest for the current assembly.
		////
		//// Returns:
		////     The module that contains the manifest for the assembly.
		public override Module ManifestModule { get; }
		////
		//// Summary:
		////     Gets a collection that contains the modules in this assembly.
		////
		//// Returns:
		////     A collection that contains the modules in this assembly.
		//public override IEnumerable<Module> Modules { get; }
		////
		//// Summary:
		////     Gets a System.Boolean value indicating whether this assembly was loaded into
		////     the reflection-only context.
		////
		//// Returns:
		////     true if the assembly was loaded into the reflection-only context, rather than
		////     the execution context; otherwise, false.
		//public override bool ReflectionOnly { get; }
		////
		//// Summary:
		////     Gets a string representing the version of the common language runtime (CLR) saved
		////     in the file containing the manifest.
		////
		//// Returns:
		////     The CLR version folder name. This is not a full path.
		//public override string ImageRuntimeVersion { get; }
		////
		//// Summary:
		////     Gets a value that indicates which set of security rules the common language runtime
		////     (CLR) enforces for this assembly.
		////
		//// Returns:
		////     The security rule set that the CLR enforces for this assembly.
		//public override SecurityRuleSet SecurityRuleSet { get; }
		//
		////
		//// Summary:
		////     Locates the specified type from this assembly and creates an instance of it using
		////     the system activator, with optional case-sensitive search and having the specified
		////     culture, arguments, and binding and activation attributes.
		////
		//// Parameters:
		////   typeName:
		////     The System.Type.FullName of the type to locate.
		////
		////   ignoreCase:
		////     true to ignore the case of the type name; otherwise, false.
		////
		////   bindingAttr:
		////     A bitmask that affects the way in which the search is conducted. The value is
		////     a combination of bit flags from System.Reflection.BindingFlags.
		////
		////   binder:
		////     An object that enables the binding, coercion of argument types, invocation of
		////     members, and retrieval of MemberInfo objects via reflection. If binder is null,
		////     the default binder is used.
		////
		////   args:
		////     An array that contains the arguments to be passed to the constructor. This array
		////     of arguments must match in number, order, and type the parameters of the constructor
		////     to be invoked. If the parameterless constructor is desired, args must be an empty
		////     array or null.
		////
		////   culture:
		////     An instance of CultureInfo used to govern the coercion of types. If this is null,
		////     the System.Globalization.CultureInfo for the current thread is used. (This is
		////     necessary to convert a string that represents 1000 to a System.Double value,
		////     for example, since 1000 is represented differently by different cultures.)
		////
		////   activationAttributes:
		////     An array of one or more attributes that can participate in activation. Typically,
		////     an array that contains a single System.Runtime.Remoting.Activation.UrlAttribute
		////     object that specifies the URL that is required to activate a remote object. This
		////     parameter is related to client-activated objects. Client activation is a legacy
		////     technology that is retained for backward compatibility but is not recommended
		////     for new development. Distributed applications should instead use Windows Communication
		////     Foundation.
		////
		//// Returns:
		////     An instance of the specified type, or null if typeName is not found. The supplied
		////     arguments are used to resolve the type, and to bind the constructor that is used
		////     to create the instance.
		////
		//// Exceptions:
		////   T:System.ArgumentException:
		////     typeName is an empty string ("") or a string beginning with a null character.
		////     -or- The current assembly was loaded into the reflection-only context.
		////
		////   T:System.ArgumentNullException:
		////     typeName is null.
		////
		////   T:System.MissingMethodException:
		////     No matching constructor was found.
		////
		////   T:System.NotSupportedException:
		////     A non-empty activation attributes array is passed to a type that does not inherit
		////     from System.MarshalByRefObject.
		////
		////   T:System.IO.FileNotFoundException:
		////     typeName requires a dependent assembly that could not be found.
		////
		////   T:System.IO.FileLoadException:
		////     typeName requires a dependent assembly that was found but could not be loaded.
		////     -or- The current assembly was loaded into the reflection-only context, and typeName
		////     requires a dependent assembly that was not preloaded.
		////
		////   T:System.BadImageFormatException:
		////     typeName requires a dependent assembly, but the file is not a valid assembly.
		////     -or- typeName requires a dependent assembly which that was compiled for a version
		////     of the runtime that is later than the currently loaded version.
		//public override object? CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder? binder, object[]? args, CultureInfo? culture, object[]? activationAttributes);
		//
		////
		//// Summary:
		////     Gets the custom attributes for this assembly as specified by type.
		////
		//// Parameters:
		////   attributeType:
		////     The type for which the custom attributes are to be returned.
		////
		////   inherit:
		////     This argument is ignored for objects of type System.Reflection.Assembly.
		////
		//// Returns:
		////     An array that contains the custom attributes for this assembly as specified by
		////     attributeType.
		////
		//// Exceptions:
		////   T:System.ArgumentNullException:
		////     attributeType is null.
		////
		////   T:System.ArgumentException:
		////     attributeType is not a runtime type.
		//public override object[] GetCustomAttributes(Type attributeType, bool inherit);
		////
		//// Summary:
		////     Gets all the custom attributes for this assembly.
		////
		//// Parameters:
		////   inherit:
		////     This argument is ignored for objects of type System.Reflection.Assembly.
		////
		//// Returns:
		////     An array that contains the custom attributes for this assembly.
		//public override object[] GetCustomAttributes(bool inherit);
		////
		//// Summary:
		////     Returns information about the attributes that have been applied to the current
		////     System.Reflection.Assembly, expressed as System.Reflection.CustomAttributeData
		////     objects.
		////
		//// Returns:
		////     A generic list of System.Reflection.CustomAttributeData objects representing
		////     data about the attributes that have been applied to the current assembly.
		//public override IList<CustomAttributeData> GetCustomAttributesData();
		////
		//// Summary:
		////     Gets the public types defined in this assembly that are visible outside the assembly.
		////
		//// Returns:
		////     An array that represents the types defined in this assembly that are visible
		////     outside the assembly.
		////
		//// Exceptions:
		////   T:System.NotSupportedException:
		////     The assembly is a dynamic assembly.
		////
		////   T:System.IO.FileNotFoundException:
		////     Unable to load a dependent assembly.
		//public override Type[] GetExportedTypes();
		////
		//// Summary:
		////     Gets a System.IO.FileStream for the specified file in the file table of the manifest
		////     of this assembly.
		////
		//// Parameters:
		////   name:
		////     The name of the specified file. Do not include the path to the file.
		////
		//// Returns:
		////     A stream that contains the specified file, or null if the file is not found.
		////
		//// Exceptions:
		////   T:System.IO.FileLoadException:
		////     A file that was found could not be loaded.
		////
		////   T:System.ArgumentNullException:
		////     The name parameter is null.
		////
		////   T:System.ArgumentException:
		////     The name parameter is an empty string ("").
		////
		////   T:System.IO.FileNotFoundException:
		////     name was not found.
		////
		////   T:System.BadImageFormatException:
		////     name is not a valid assembly.
		//public override FileStream? GetFile(string name);
		////
		//// Summary:
		////     Gets the files in the file table of an assembly manifest.
		////
		//// Returns:
		////     An array of streams that contain the files.
		////
		//// Exceptions:
		////   T:System.IO.FileLoadException:
		////     A file that was found could not be loaded.
		////
		////   T:System.IO.FileNotFoundException:
		////     A file was not found.
		////
		////   T:System.BadImageFormatException:
		////     A file was not a valid assembly.
		//public override FileStream[] GetFiles();
		////
		//// Summary:
		////     Gets the files in the file table of an assembly manifest, specifying whether
		////     to include resource modules.
		////
		//// Parameters:
		////   getResourceModules:
		////     true to include resource modules; otherwise, false.
		////
		//// Returns:
		////     An array of streams that contain the files.
		////
		//// Exceptions:
		////   T:System.IO.FileLoadException:
		////     A file that was found could not be loaded.
		////
		////   T:System.IO.FileNotFoundException:
		////     A file was not found.
		////
		////   T:System.BadImageFormatException:
		////     A file was not a valid assembly.
		//public override FileStream[] GetFiles(bool getResourceModules);
		//
		//public override Type[] GetForwardedTypes();
		////
		//// Summary:
		////     Returns the hash code for this instance.
		////
		//// Returns:
		////     A 32-bit signed integer hash code.
		//public override int GetHashCode();
		////
		//// Summary:
		////     Gets all the loaded modules that are part of this assembly, specifying whether
		////     to include resource modules.
		////
		//// Parameters:
		////   getResourceModules:
		////     true to include resource modules; otherwise, false.
		////
		//// Returns:
		////     An array of modules.
		//public override Module[] GetLoadedModules(bool getResourceModules);
		//
		////
		//// Summary:
		////     Returns information about how the given resource has been persisted.
		////
		//// Parameters:
		////   resourceName:
		////     The case-sensitive name of the resource.
		////
		//// Returns:
		////     An object that is populated with information about the resource's topology, or
		////     null if the resource is not found.
		////
		//// Exceptions:
		////   T:System.ArgumentNullException:
		////     resourceName is null.
		////
		////   T:System.ArgumentException:
		////     The resourceName parameter is an empty string ("").
		//public override ManifestResourceInfo? GetManifestResourceInfo(string resourceName);
		////
		//// Summary:
		////     Returns the names of all the resources in this assembly.
		////
		//// Returns:
		////     An array that contains the names of all the resources.
		//public override string[] GetManifestResourceNames();
		////
		//// Summary:
		////     Loads the specified manifest resource, scoped by the namespace of the specified
		////     type, from this assembly.
		////
		//// Parameters:
		////   type:
		////     The type whose namespace is used to scope the manifest resource name.
		////
		////   name:
		////     The case-sensitive name of the manifest resource being requested.
		////
		//// Returns:
		////     The manifest resource; or null if no resources were specified during compilation
		////     or if the resource is not visible to the caller.
		////
		//// Exceptions:
		////   T:System.ArgumentNullException:
		////     The name parameter is null.
		////
		////   T:System.ArgumentException:
		////     The name parameter is an empty string ("").
		////
		////   T:System.IO.FileLoadException:
		////     A file that was found could not be loaded.
		////
		////   T:System.IO.FileNotFoundException:
		////     name was not found.
		////
		////   T:System.BadImageFormatException:
		////     name is not a valid assembly.
		////
		////   T:System.NotImplementedException:
		////     Resource length is greater than System.Int64.MaxValue.
		//public override Stream? GetManifestResourceStream(Type type, string name);
		////
		//// Summary:
		////     Loads the specified manifest resource from this assembly.
		////
		//// Parameters:
		////   name:
		////     The case-sensitive name of the manifest resource being requested.
		////
		//// Returns:
		////     The manifest resource; or null if no resources were specified during compilation
		////     or if the resource is not visible to the caller.
		////
		//// Exceptions:
		////   T:System.ArgumentNullException:
		////     The name parameter is null.
		////
		////   T:System.ArgumentException:
		////     The name parameter is an empty string ("").
		////
		////   T:System.IO.FileLoadException:
		////     In the .NET for Windows Store apps or the Portable Class Library, catch the base
		////     class exception, System.IO.IOException, instead.
		////     A file that was found could not be loaded.
		////
		////   T:System.IO.FileNotFoundException:
		////     name was not found.
		////
		////   T:System.BadImageFormatException:
		////     name is not a valid assembly.
		////
		////   T:System.NotImplementedException:
		////     Resource length is greater than System.Int64.MaxValue.
		//public override Stream? GetManifestResourceStream(string name);
		////
		//// Summary:
		////     Gets the specified module in this assembly.
		////
		//// Parameters:
		////   name:
		////     The name of the module being requested.
		////
		//// Returns:
		////     The module being requested, or null if the module is not found.
		////
		//// Exceptions:
		////   T:System.ArgumentNullException:
		////     The name parameter is null.
		////
		////   T:System.ArgumentException:
		////     The name parameter is an empty string ("").
		////
		////   T:System.IO.FileLoadException:
		////     A file that was found could not be loaded.
		////
		////   T:System.IO.FileNotFoundException:
		////     name was not found.
		////
		////   T:System.BadImageFormatException:
		////     name is not a valid assembly.
		//public override Module? GetModule(string name);
		//
		////
		//// Summary:
		////     Gets all the modules that are part of this assembly, specifying whether to include
		////     resource modules.
		////
		//// Parameters:
		////   getResourceModules:
		////     true to include resource modules; otherwise, false.
		////
		//// Returns:
		////     An array of modules.
		//public override Module[] GetModules(bool getResourceModules);
		////
		//// Summary:
		////     Gets an System.Reflection.AssemblyName for this assembly, setting the codebase
		////     as specified by copiedName.
		////
		//// Parameters:
		////   copiedName:
		////     true to set the System.Reflection.Assembly.CodeBase to the location of the assembly
		////     after it was shadow copied; false to set System.Reflection.Assembly.CodeBase
		////     to the original location.
		////
		//// Returns:
		////     An object that contains the fully parsed display name for this assembly.
		//public override AssemblyName GetName(bool copiedName);
		////
		//// Summary:
		////     Gets an System.Reflection.AssemblyName for this assembly.
		////
		//// Returns:
		////     An object that contains the fully parsed display name for this assembly.
		//public override AssemblyName GetName();
		//
		//public override AssemblyName[] GetReferencedAssemblies();
		////
		//// Summary:
		////     Gets the specified version of the satellite assembly for the specified culture.
		////
		//// Parameters:
		////   culture:
		////     The specified culture.
		////
		////   version:
		////     The version of the satellite assembly.
		////
		//// Returns:
		////     The specified satellite assembly.
		////
		//// Exceptions:
		////   T:System.ArgumentNullException:
		////     culture is null.
		////
		////   T:System.IO.FileLoadException:
		////     The satellite assembly with a matching file name was found, but the CultureInfo
		////     or the version did not match the one specified.
		////
		////   T:System.IO.FileNotFoundException:
		////     The assembly cannot be found.
		////
		////   T:System.BadImageFormatException:
		////     The satellite assembly is not a valid assembly.
		//public override Assembly GetSatelliteAssembly(CultureInfo culture, Version? version);
		////
		//// Summary:
		////     Gets the satellite assembly for the specified culture.
		////
		//// Parameters:
		////   culture:
		////     The specified culture.
		////
		//// Returns:
		////     The specified satellite assembly.
		////
		//// Exceptions:
		////   T:System.ArgumentNullException:
		////     culture is null.
		////
		////   T:System.IO.FileNotFoundException:
		////     The assembly cannot be found.
		////
		////   T:System.IO.FileLoadException:
		////     The satellite assembly with a matching file name was found, but the CultureInfo
		////     did not match the one specified.
		////
		////   T:System.BadImageFormatException:
		////     The satellite assembly is not a valid assembly.
		//public override Assembly GetSatelliteAssembly(CultureInfo culture);

		public override Type? GetType(string name, bool throwOnError, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			Type? result = GetTypes().FirstOrDefault(x => string.Equals(x.FullName, name, comparison));
			if (throwOnError && (result == null))
				throw new TypeLoadException();

			return result;
		}

		public override Type? GetType(string name)
		  => GetType(name, false, false);

		public override Type? GetType(string name, bool throwOnError)
		  => GetType(name, throwOnError, false);

		public override Type[] GetTypes()
		  => ManifestModule.GetTypes();

		////
		//// Summary:
		////     Indicates whether or not a specified attribute has been applied to the assembly.
		////
		//// Parameters:
		////   attributeType:
		////     The type of the attribute to be checked for this assembly.
		////
		////   inherit:
		////     This argument is ignored for objects of this type.
		////
		//// Returns:
		////     true if the attribute has been applied to the assembly; otherwise, false.
		////
		//// Exceptions:
		////   T:System.ArgumentNullException:
		////     attributeType is null.
		////
		////   T:System.ArgumentException:
		////     attributeType uses an invalid type.
		//public override bool IsDefined(Type attributeType, bool inherit);
	}
}
