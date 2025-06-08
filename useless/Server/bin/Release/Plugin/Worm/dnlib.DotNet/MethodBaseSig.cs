using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class MethodBaseSig : CallingConventionSig
{
	protected TypeSig retType;

	protected IList<TypeSig> parameters;

	protected uint genParamCount;

	protected IList<TypeSig> paramsAfterSentinel;

	public CallingConvention CallingConvention
	{
		get
		{
			return callingConvention;
		}
		set
		{
			callingConvention = value;
		}
	}

	public TypeSig RetType
	{
		get
		{
			return retType;
		}
		set
		{
			retType = value;
		}
	}

	public IList<TypeSig> Params => parameters;

	public uint GenParamCount
	{
		get
		{
			return genParamCount;
		}
		set
		{
			genParamCount = value;
		}
	}

	public IList<TypeSig> ParamsAfterSentinel
	{
		get
		{
			return paramsAfterSentinel;
		}
		set
		{
			paramsAfterSentinel = value;
		}
	}
}
