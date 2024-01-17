using System;
using System.Reflection;

namespace TurtleNet.Reflection
{
	sealed class InterpretedParameter : ParameterInfo
	{
		public InterpretedParameter(string? name, Type parameterType)
		{
			Name = name;
			ParameterType = parameterType;
		}

		public override string? Name { get; }
		public override Type ParameterType { get; }
	}
}