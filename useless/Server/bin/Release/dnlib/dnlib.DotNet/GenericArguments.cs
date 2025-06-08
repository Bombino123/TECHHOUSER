using System.Collections.Generic;

namespace dnlib.DotNet;

internal sealed class GenericArguments
{
	private GenericArgumentsStack typeArgsStack = new GenericArgumentsStack(isTypeVar: true);

	private GenericArgumentsStack methodArgsStack = new GenericArgumentsStack(isTypeVar: false);

	public void PushTypeArgs(IList<TypeSig> typeArgs)
	{
		typeArgsStack.Push(typeArgs);
	}

	public IList<TypeSig> PopTypeArgs()
	{
		return typeArgsStack.Pop();
	}

	public void PushMethodArgs(IList<TypeSig> methodArgs)
	{
		methodArgsStack.Push(methodArgs);
	}

	public IList<TypeSig> PopMethodArgs()
	{
		return methodArgsStack.Pop();
	}

	public TypeSig Resolve(TypeSig typeSig)
	{
		if (typeSig == null)
		{
			return null;
		}
		if (typeSig is GenericMVar genericMVar)
		{
			TypeSig typeSig2 = methodArgsStack.Resolve(genericMVar.Number);
			if (typeSig2 == null || typeSig2 == typeSig)
			{
				return typeSig;
			}
			return typeSig2;
		}
		if (!(typeSig is GenericVar genericVar))
		{
			return typeSig;
		}
		TypeSig typeSig3 = typeArgsStack.Resolve(genericVar.Number);
		if (typeSig3 == null || typeSig3 == typeSig)
		{
			return typeSig;
		}
		return typeSig3;
	}
}
