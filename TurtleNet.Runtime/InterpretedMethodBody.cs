#region using
using System;
using System.Reflection;
#endregion using

namespace TurtleNet.Runtime.Internal
{
	public sealed class InterpretedMethodBody : MethodBody
	{
		public InterpretedMethodBody(IlInstruction[] instructions, int maxStackSize, Type[]? locals)
		{
			_instructions = instructions;
			_maxStackSize = maxStackSize;
			_locals = locals;
		}

		readonly int _maxStackSize;
		public override int MaxStackSize => _maxStackSize;

		readonly IlInstruction[] _instructions;
		readonly Type[]? _locals;

		public object? Invoke(Type returnType, object? obj, object?[]? parameters)
		  => RuntimeInterpreter.Invoke(_instructions, _maxStackSize, obj, returnType, parameters, _locals);
	}
}
