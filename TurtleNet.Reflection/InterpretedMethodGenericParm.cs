using System.Reflection;

namespace TurtleNet.Reflection
{
	sealed class InterpretedMethodGenericParm : InterpretedGenericParmBase
	{
		internal InterpretedMethodGenericParm(int index, string name, GenericParameterAttributes genericParameterAttributes) : base(index, name, genericParameterAttributes)
		{
		}

		MethodBase? _declaringMethod;
		public override MethodBase? DeclaringMethod => _declaringMethod;

		internal void SetDeclaringMethod(MethodInfo declaringMethod)
		  => _declaringMethod = declaringMethod;

		public override bool IsGenericParameter
		  => true;
	}
}