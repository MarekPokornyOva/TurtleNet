namespace System.Reflection
{
	internal static class Throw
	{
		internal static void ArgumentNull(string parameterName)
		{
			throw new ArgumentNullException(parameterName);
		}
	}
}
