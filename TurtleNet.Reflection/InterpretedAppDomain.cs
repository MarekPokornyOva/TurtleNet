using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TurtleNet.Reflection
{
	public sealed class InterpretedAppDomain
	{
		InterpretedAppDomain()
		{
		}

		public static InterpretedAppDomain CurrentDomain { get; } = new InterpretedAppDomain();

		ResolveEventHandler? _assemblyResolve;
		public event ResolveEventHandler AssemblyResolve
		{
			add
			{
				lock (this)
					_assemblyResolve = (ResolveEventHandler)Delegate.Combine(_assemblyResolve, value);
			}
			remove
			{
				lock (this)
					_assemblyResolve = (ResolveEventHandler)Delegate.Remove(_assemblyResolve, value);
			}
		}

		readonly Dictionary<AssemblyName, Assembly> _cache = new Dictionary<AssemblyName, Assembly>();
		public Assembly Load(AssemblyName assemblyName)
		{
			Assembly? result = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName() == assemblyName);
			if (result != null)
				return result!;

			lock (this)
			{
				if (_cache.TryGetValue(assemblyName, out result))
					return result!;

				result = _assemblyResolve?.Invoke(this, new ResolveEventArgs(assemblyName.FullName))
					?? throw new FileNotFoundException(assemblyName + " not found.");
				_cache[assemblyName] = result;
				return result;
			}
		}

		public Assembly[] GetAssemblies()
			=> _cache.Values.ToArray();
	}
}
