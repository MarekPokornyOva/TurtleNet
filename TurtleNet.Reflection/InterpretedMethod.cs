using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TurtleNet.Runtime;
using TurtleNet.Runtime.Internal;

namespace TurtleNet.Reflection
{
	interface IInterpretedMethodBase
	{
		int Address { get; }

		void SetBody(InterpretedMethodBody interpretedMethodBody);
	}

	sealed class InterpretedMethod : MethodInfo, IInterpretedMethodBase
	{
		internal InterpretedMethod(int address, string name, Type declaringType, InterpretedMethodGenericParm[] genericParms, Type returnType, ParameterInfo[] parameters)
		{
			Address = address;
			Name = name;
			DeclaringType = declaringType;
			ReturnType = returnType;
			_parameters = parameters;
			_genericMethodArguments = genericParms;
		}

		int IInterpretedMethodBase.Address => Address;
		internal int Address { get; }

		public override MethodInfo GetBaseDefinition()
		{
			throw new NotImplementedException();
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes { get; }

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			throw new NotImplementedException();
		}

		ParameterInfo[] _parameters;
		public override ParameterInfo[] GetParameters()
		  => _parameters;

		public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture)
		  => _body.Invoke(ReturnType, obj, parameters);

		public override MethodAttributes Attributes { get; }
		public override RuntimeMethodHandle MethodHandle { get; }

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		Type[] _genericMethodArguments;
		public override bool ContainsGenericParameters => _genericMethodArguments.Length != 0;
		public override Type[] GetGenericArguments() => _genericMethodArguments;
		public override bool IsGenericMethod => ContainsGenericParameters;
		public override bool IsGenericMethodDefinition => ContainsGenericParameters;

		public override Type? DeclaringType { get; }
		public override string Name { get; }
		public override Type? ReflectedType { get; }

		public override Type ReturnType { get; }

		InterpretedMethodBody? _body;
		public override MethodBody? GetMethodBody()
		  => _body;

		void IInterpretedMethodBase.SetBody(InterpretedMethodBody? body)
		  => _body = body;
	}
}
