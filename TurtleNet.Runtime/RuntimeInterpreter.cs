#region using
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using TurtleNet.Runtime.Internal;
#endregion using

namespace TurtleNet.Runtime
{
	static class RuntimeInterpreter
	{
		internal static object? Invoke(IlInstruction[] instructions, int maxStackSize, object? obj, Type returnType, object?[]? parameters, Type[]? localTypes)
		{
			Stack<object?> stack = new Stack<object?>(maxStackSize);
			object?[]? locals = localTypes == null ? null : new object[localTypes.Length];
			object? result = null;

			int instrIndex;
			for (instrIndex = 0; instrIndex < instructions.Length; instrIndex++)
			{
				IlInstruction instruction = instructions[instrIndex];

				switch (instruction.Op)
				{
					case ILOpCode.Nop:
						break;
					case ILOpCode.Ret:
						if (returnType != typeof(void))
							result = Convert.ChangeType(stack.Pop(), returnType);
						break;
					case ILOpCode.Ldstr:
						stack.Push(instruction.Data);
						break;
					case ILOpCode.Call:
						MethodInfo mi = (MethodInfo)instruction.Data;
						int parmsLen = mi.GetParameters().Length;
						object?[] parms = new object[parmsLen];
						for (int a = parmsLen - 1; a >= 0; a--)
							parms[a] = stack.Pop();
						object? res = mi.Invoke(null, parms);
						if (mi.ReturnType != typeof(void))
							stack.Push(res);
						break;
					case ILOpCode.Ldarg_0:
						stack.Push(parameters![0]);
						break;
					case ILOpCode.Ldarg_1:
						stack.Push(parameters![1]);
						break;
					case ILOpCode.Add:
						RuntimeMethods.Add(stack);
						break;
					case ILOpCode.Ldc_i4_0:
						stack.Push(0);
						break;
					case ILOpCode.Ldc_i4_1:
						stack.Push(1);
						break;
					case ILOpCode.Stloc_0:
						locals![0] = stack.Pop();
						break;
					case ILOpCode.Ldloc_0:
						stack.Push(locals![0]);
						break;
					case ILOpCode.Box: //all is handled as objects so no action needed.
						break;
					case ILOpCode.Br_s:
						CalculateTargetInstructionIndex(instructions, instruction, ref instrIndex);
						break;
					case ILOpCode.Brfalse_s:
						if (RuntimeMethods.IsFalse(stack))
							CalculateTargetInstructionIndex(instructions, instruction, ref instrIndex);
						break;
					default:
						throw new NotImplementedException();
				}
			}
			return result;
		}

		static void CalculateTargetInstructionIndex(IlInstruction[] instructions, IlInstruction instruction, ref int instrIndex)
		{
			int offset = (instruction.Data is byte b ? (int)b : (int)instruction.Data);
			if (offset == 0)
				return;

			//get end of instruction...
			//int newAddress = instruction.Address + 1/*operand bytes count - BR family is only 1 byte long*/ + instruction.RawData.Length
			//  //... plus offset
			//  + offset;
			int newAddress = instructions[instrIndex + 1].Address + offset;
			instrIndex = Array.FindIndex(instructions, x => x.Address == newAddress) - 1;
		}
	}
}
