using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using TurtleNet.Runtime.Internal;

namespace TurtleNet.Reflection
{
	internal class IlReader
	{
		static readonly Dictionary<ushort, ILOpCode> _instructionLookup = new Dictionary<ushort, ILOpCode>();
		Module _module;
		bool _declaringTypeIsGeneric;
		Type[]? _typeGenericArguments;
		bool _methodIsGeneric;
		Type[]? _methodGenericArguments;

		static IlReader()
		{
			FillLookupTable();
		}

		internal IlInstruction[] ReadMethod(byte[] instructionBytes, Type declaringType, Type[] methodGenericArguments, Module module)
		{
			_module = module;
			_declaringTypeIsGeneric = declaringType.IsGenericType;
			_typeGenericArguments = declaringType.GetGenericArguments();
			_methodIsGeneric = (methodGenericArguments?.Length ?? 0) != 0;
			_methodGenericArguments = methodGenericArguments;

			List<IlInstruction> result = new List<IlInstruction>();

			int instructionIndex = 0;
			int startAddress;
			for (int position = 0; position < instructionBytes.Length;)
			{
				startAddress = position;

				ushort operationData = instructionBytes[position];
				if (IsInstructionPrefix(operationData))
					operationData = (ushort)((operationData << 8) | instructionBytes[++position]);

				position++;

				if (!_instructionLookup.TryGetValue(operationData, out ILOpCode code))
					throw new InvalidProgramException(string.Format("0x{0:X2} is not a valid op code.", operationData));
				OperandType operandType = GetOperandType(code);

				int dataSize = GetDataSize(operandType);
				byte[] data = new byte[dataSize];
				Buffer.BlockCopy(instructionBytes, position, data, 0, dataSize);

				object? objData = this.GetData(operandType, data);
				position += dataSize;

				if (operandType == OperandType.InlineSwitch)
				{
					dataSize = (int)objData!;
					int[] labels = new int[dataSize];
					for (int index = 0; index < labels.Length; index++)
					{
						labels[index] = BitConverter.ToInt32(instructionBytes, position);
						position += 4;
					}

					objData = labels;
				}

				result.Add(new IlInstruction(code, data, startAddress, objData, instructionIndex++));
			}

			return result.ToArray();
		}

		static bool IsInstructionPrefix(ushort value)
			=> ((value & OpCodes.Prefix1.Value) == OpCodes.Prefix1.Value) || ((value & OpCodes.Prefix2.Value) == OpCodes.Prefix2.Value)
				|| ((value & OpCodes.Prefix3.Value) == OpCodes.Prefix3.Value) || ((value & OpCodes.Prefix4.Value) == OpCodes.Prefix4.Value)
				|| ((value & OpCodes.Prefix5.Value) == OpCodes.Prefix5.Value) || ((value & OpCodes.Prefix6.Value) == OpCodes.Prefix6.Value)
				|| ((value & OpCodes.Prefix7.Value) == OpCodes.Prefix7.Value) || ((value & OpCodes.Prefixref.Value) == OpCodes.Prefixref.Value);

		static OperandType GetOperandType(ILOpCode opCode)
		{
			int position = 0;
			byte b1 = (byte)(((ushort)opCode) >> 8);
			byte b2 = (byte)(((ushort)opCode) & 255);
			byte[] opcodeBytes = b1 == 0xfe ? new byte[] { b1, b2 } : new byte[] { b2, 0 };
			return InstructionOperandTypes.ReadOperandType(opcodeBytes, ref position);
		}

		static int GetDataSize(OperandType operandType)
		{
			switch (operandType)
			{
				case OperandType.InlineNone:
					return 0;
				case OperandType.ShortInlineBrTarget:
				case OperandType.ShortInlineI:
				case OperandType.ShortInlineVar:
					return 1;
				case OperandType.InlineVar:
					return 2;
				case OperandType.InlineBrTarget:
				case OperandType.InlineField:
				case OperandType.InlineI:
				case OperandType.InlineMethod:
				case OperandType.InlineSig:
				case OperandType.InlineString:
				case OperandType.InlineSwitch:
				case OperandType.InlineTok:
				case OperandType.InlineType:
				case OperandType.ShortInlineR:
					return 4;
				case OperandType.InlineI8:
				case OperandType.InlineR:
					return 8;
				default:
					return 0;
			}
		}

		private object? GetData(OperandType operandType, byte[] rawData)
		{
			object? data = null;
			switch (operandType)
			{
				case OperandType.InlineField:
					if ((_declaringTypeIsGeneric) || (_methodIsGeneric))
						data = _module.ResolveField(BitConverter.ToInt32(rawData, 0), _typeGenericArguments, _methodGenericArguments);
					else
						data = _module.ResolveField(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineSwitch:
					data = BitConverter.ToInt32(rawData, 0);
					break;
				case OperandType.InlineBrTarget:
				case OperandType.InlineI:
					data = BitConverter.ToInt32(rawData, 0);
					break;
				case OperandType.InlineI8:
					data = BitConverter.ToInt64(rawData, 0);
					break;
				case OperandType.InlineMethod:
					if ((_declaringTypeIsGeneric) || (_methodIsGeneric))
						data = _module.ResolveMethod(BitConverter.ToInt32(rawData, 0), _typeGenericArguments, _methodGenericArguments);
					else
						data = _module.ResolveMethod(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineR:
					data = BitConverter.ToDouble(rawData, 0);
					break;
				case OperandType.InlineSig:
					data = _module.ResolveSignature(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineString:
					data = _module.ResolveString(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineTok:
				case OperandType.InlineType:
					if ((_declaringTypeIsGeneric) || (_methodIsGeneric))
						data = _module.ResolveType(BitConverter.ToInt32(rawData, 0), _typeGenericArguments, _methodGenericArguments);
					else
						data = _module.ResolveType(BitConverter.ToInt32(rawData, 0));
					break;
				case OperandType.InlineVar:
					data = BitConverter.ToInt16(rawData, 0);
					break;
				case OperandType.ShortInlineVar:
				case OperandType.ShortInlineI:
				case OperandType.ShortInlineBrTarget:
					data = rawData[0];
					break;
				case OperandType.ShortInlineR:
					data = BitConverter.ToSingle(rawData, 0);
					break;
			}
			return data;
		}

		static void FillLookupTable()
		{
			// Might be better to do an array lookup.  Use a seperate arrary for instructions without a prefix and array for each prefix.
			FieldInfo[] fields = typeof(ILOpCode).GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				ILOpCode code = (ILOpCode)field.GetValue(null);
				_instructionLookup.Add((ushort)code, code);
			}
		}
	}
}
