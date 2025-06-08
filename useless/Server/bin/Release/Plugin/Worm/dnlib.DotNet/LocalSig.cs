using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class LocalSig : CallingConventionSig
{
	private readonly IList<TypeSig> locals;

	public IList<TypeSig> Locals => locals;

	public LocalSig()
	{
		callingConvention = CallingConvention.LocalSig;
		locals = new List<TypeSig>();
	}

	internal LocalSig(CallingConvention callingConvention, uint count)
	{
		base.callingConvention = callingConvention;
		locals = new List<TypeSig>((int)count);
	}

	public LocalSig(TypeSig local1)
	{
		callingConvention = CallingConvention.LocalSig;
		locals = new List<TypeSig> { local1 };
	}

	public LocalSig(TypeSig local1, TypeSig local2)
	{
		callingConvention = CallingConvention.LocalSig;
		locals = new List<TypeSig> { local1, local2 };
	}

	public LocalSig(TypeSig local1, TypeSig local2, TypeSig local3)
	{
		callingConvention = CallingConvention.LocalSig;
		locals = new List<TypeSig> { local1, local2, local3 };
	}

	public LocalSig(params TypeSig[] locals)
	{
		callingConvention = CallingConvention.LocalSig;
		this.locals = new List<TypeSig>(locals);
	}

	public LocalSig(IList<TypeSig> locals)
	{
		callingConvention = CallingConvention.LocalSig;
		this.locals = new List<TypeSig>(locals);
	}

	internal LocalSig(IList<TypeSig> locals, bool dummy)
	{
		callingConvention = CallingConvention.LocalSig;
		this.locals = locals;
	}

	public LocalSig Clone()
	{
		return new LocalSig(locals);
	}
}
