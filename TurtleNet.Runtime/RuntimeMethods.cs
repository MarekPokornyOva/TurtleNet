using System.Collections.Generic;

namespace TurtleNet.Runtime
{
	static class RuntimeMethods
	{
		internal static void Add(Stack<object?> stack)
		{
			object? y = stack.Pop();
			stack.Push((int)stack.Pop()! + (int)y!);
		}

		internal static bool IsFalse(Stack<object?> stack)
		{
			object? val = stack.Pop();
			return (val == null) || (val is int i && i == 0) || (val is bool b && !b);
		}
	}
}
