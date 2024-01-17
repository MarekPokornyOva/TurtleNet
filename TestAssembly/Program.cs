using System;

namespace TestAssembly
{
	public class Program
	{
		static void Main()
		{
			Console.WriteLine("Hello World!");
		}

		public static int Add(int x, int y)
		  => x + y;

		//public static bool Equals<T>(T x, T y) where T : IEquatable<T>
		//  => x.Equals(y);
	}

	public static class Gen<T>
	{
		public static bool Equals2(int x, T y)
		  => Equals(x, y);

		public static bool Equals<T2>(T2 x, T y)
			=> x == null ? true : false;
	}
}
