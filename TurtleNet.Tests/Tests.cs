#region using
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jellequin.Reflection.Emit;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using TurtleNet.Reflection;
using System.Linq;
#endregion using

namespace TurtleNet.Tests
{
	[TestClass]
	[TestCategory("Tests")]
	public class Tests
	{
		[TestMethod]
		public void TestDynamicAssembly()
		{
			#region CreateAssembly
			static Assembly CreateAssembly()
			{
				ModuleBuilder modBldr = new ModuleBuilder(new AssemblyName("TestAssemblyDynam"));
				TypeBuilder typeBldr = modBldr.DefineType("Program", "TestAssembly", TypeAttributes.Public, typeof(object), Type.EmptyTypes);
				MethodBuilder mBldr = typeBldr.DefineMethod("Add", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard);
				mBldr.SetReturnType(typeof(int));
				mBldr.SetParameters(new[] { typeof(int), typeof(int) });
				mBldr.DefineParameter(1, ParameterAttributes.None, "x");
				mBldr.DefineParameter(2, ParameterAttributes.None, "y");
				ILGenerator gen = mBldr.GetILGenerator();
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldarg_1);
				gen.Emit(ILOpCode.Add);
				gen.Emit(ILOpCode.Ret);

				byte[] dll;
				using (MemoryStream ms = new MemoryStream())
				{
					new AssemblyBuilder(modBldr, null)
					  .Save(ms, new SaveOptions());
					dll = ms.ToArray();
				}

				return InterpretedAssembly.Load(dll);
			}
			#endregion CreateAssembly

			Assembly asm = CreateAssembly();
			object? res = asm.GetType("TestAssembly.Program").GetMethod("Add", BindingFlags.Public | BindingFlags.Static)
			  .Invoke(null, new object[] { 2, 3 });

			Assert.IsInstanceOfType<int>(res);
			Assert.AreEqual(5, res);
		}

		[TestMethod]
		public void TestAssembly1()
		{
			InterpretedAppDomain.CurrentDomain.AssemblyResolve += (e, args) => AppDomain.CurrentDomain.Load(new AssemblyName(args.Name));
			Assembly asm = InterpretedAssembly.Load(File.ReadAllBytes(typeof(TestAssembly.Program).Assembly.Location));
			
			MethodInfo entryMethod = asm.EntryPoint;
			entryMethod?.Invoke(null, new object[] { Array.Empty<string>() });

			object? res = asm.GetType("TestAssembly.Program").GetMethod("Add", BindingFlags.Public | BindingFlags.Static)
			  .Invoke(null, new object[] { 2, 3 });

			Assert.IsInstanceOfType<int>(res);
			Assert.AreEqual(5, res);
		}

		[TestMethod]
		public void TestAssembly2()
		{
			InterpretedAppDomain.CurrentDomain.AssemblyResolve += (e, args) => AppDomain.CurrentDomain.Load(new AssemblyName(args.Name));
			Assembly asm = InterpretedAssembly.Load(File.ReadAllBytes(typeof(TestAssembly.Program).Assembly.Location));

			Type t = asm.GetType("TestAssembly.Gen`1");
			MethodInfo mi = t.GetMethods(BindingFlags.Public | BindingFlags.Static).First(x => x.Name == "Equals2");

			object? res = mi.Invoke(null, new object[] { 3, 8 });
			Assert.IsInstanceOfType<bool>(res);
			Assert.AreEqual(false, res);

			res = mi.Invoke(null, new object[] { 3, 3 });
			Assert.IsInstanceOfType<bool>(res);
			Assert.AreEqual(false, res);

			res = mi.Invoke(null, new object[] { null, 3 });
			Assert.IsInstanceOfType<bool>(res);
			Assert.AreEqual(true, res);
		}
	}
}
