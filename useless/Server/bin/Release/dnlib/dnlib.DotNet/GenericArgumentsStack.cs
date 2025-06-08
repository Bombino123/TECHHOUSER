using System.Collections.Generic;

namespace dnlib.DotNet;

internal readonly struct GenericArgumentsStack
{
	private readonly List<IList<TypeSig>> argsStack;

	private readonly bool isTypeVar;

	public GenericArgumentsStack(bool isTypeVar)
	{
		argsStack = new List<IList<TypeSig>>();
		this.isTypeVar = isTypeVar;
	}

	public void Push(IList<TypeSig> args)
	{
		argsStack.Add(args);
	}

	public IList<TypeSig> Pop()
	{
		int index = argsStack.Count - 1;
		IList<TypeSig> result = argsStack[index];
		argsStack.RemoveAt(index);
		return result;
	}

	public TypeSig Resolve(uint number)
	{
		TypeSig result = null;
		for (int num = argsStack.Count - 1; num >= 0; num--)
		{
			IList<TypeSig> list = argsStack[num];
			if (number >= list.Count)
			{
				return null;
			}
			TypeSig typeSig = list[(int)number];
			if (!(typeSig is GenericSig genericSig) || genericSig.IsTypeVar != isTypeVar)
			{
				return typeSig;
			}
			result = genericSig;
			number = genericSig.Number;
		}
		return result;
	}
}
