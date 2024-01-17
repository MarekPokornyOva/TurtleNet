using System.Reflection.Metadata;
using System.Text;

namespace TurtleNet.Runtime.Internal
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct IlInstruction
	{
		private readonly ILOpCode operationCode; // 40.  56-64.  The entire structure is very big.  maybe do array lookup for opcode instead.
		private readonly byte[] instructionRawData;
		private readonly object instructionData;
		private readonly int instructionAddress;
		private readonly int index;

		public IlInstruction(ILOpCode code, byte[] instructionRawData, int instructionAddress, object instructionData, int index)
		{
			this.operationCode = code;
			this.instructionRawData = instructionRawData;
			this.instructionAddress = instructionAddress;
			this.instructionData = instructionData;
			this.index = index;
		}

		public readonly ILOpCode Op { get { return this.operationCode; } }
		public readonly byte[] RawData { get { return this.instructionRawData; } }
		public readonly object Data { get { return this.instructionData; } }
		public readonly int Address { get { return this.instructionAddress; } }
		public readonly int InstructionIndex { get { return this.index; } }

		public readonly int DataValue
		{
			get
			{
				if (this.Data != null)
				{
					if (this.Data is byte)
						return (byte)this.Data;
					else if (this.Data is short)
						return (short)this.Data;
					else if (this.Data is int)
						return (int)this.Data;
				}
				return 0;
			}
		}

		//public int Length { get { return this.Op.Size+(this.RawData==null ? 0 : this.RawData.Length); } }

#if DEBUG
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("0x{0:x4} {1,-10}", this.Address, this.Op);

			if (this.Data != null)
				builder.Append(this.Data.ToString());

			if (this.RawData != null && this.RawData.Length > 0)
			{
				builder.Append(" [0x");
				for (int i = this.RawData.Length - 1; i >= 0; i--)
					builder.Append(this.RawData[i].ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
				builder.Append(']');
			}

			return builder.ToString();
		}
#endif
	}
}
