using System.Collections.Generic;

namespace dnlib.DotNet;

public sealed class GenericInstMethodSig : CallingConventionSig
{
	private readonly IList<TypeSig> genericArgs;

	public IList<TypeSig> GenericArguments => genericArgs;

	public GenericInstMethodSig()
	{
		callingConvention = CallingConvention.GenericInst;
		genericArgs = new List<TypeSig>();
	}

	internal GenericInstMethodSig(CallingConvention callingConvention, uint size)
	{
		base.callingConvention = callingConvention;
		genericArgs = new List<TypeSig>((int)size);
	}

	public GenericInstMethodSig(TypeSig arg1)
	{
		callingConvention = CallingConvention.GenericInst;
		genericArgs = new List<TypeSig> { arg1 };
	}

	public GenericInstMethodSig(TypeSig arg1, TypeSig arg2)
	{
		callingConvention = CallingConvention.GenericInst;
		genericArgs = new List<TypeSig> { arg1, arg2 };
	}

	public GenericInstMethodSig(TypeSig arg1, TypeSig arg2, TypeSig arg3)
	{
		callingConvention = CallingConvention.GenericInst;
		genericArgs = new List<TypeSig> { arg1, arg2, arg3 };
	}

	public GenericInstMethodSig(params TypeSig[] args)
	{
		callingConvention = CallingConvention.GenericInst;
		genericArgs = new List<TypeSig>(args);
	}

	public GenericInstMethodSig(IList<TypeSig> args)
	{
		callingConvention = CallingConvention.GenericInst;
		genericArgs = new List<TypeSig>(args);
	}

	public GenericInstMethodSig Clone()
	{
		return new GenericInstMethodSig(genericArgs);
	}
}
