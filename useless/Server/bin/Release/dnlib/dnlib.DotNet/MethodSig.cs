using System.Collections.Generic;

namespace dnlib.DotNet;

public sealed class MethodSig : MethodBaseSig
{
	private uint origToken;

	public uint OriginalToken
	{
		get
		{
			return origToken;
		}
		set
		{
			origToken = value;
		}
	}

	public static MethodSig CreateStatic(TypeSig retType)
	{
		return new MethodSig(CallingConvention.Default, 0u, retType);
	}

	public static MethodSig CreateStatic(TypeSig retType, TypeSig argType1)
	{
		return new MethodSig(CallingConvention.Default, 0u, retType, argType1);
	}

	public static MethodSig CreateStatic(TypeSig retType, TypeSig argType1, TypeSig argType2)
	{
		return new MethodSig(CallingConvention.Default, 0u, retType, argType1, argType2);
	}

	public static MethodSig CreateStatic(TypeSig retType, TypeSig argType1, TypeSig argType2, TypeSig argType3)
	{
		return new MethodSig(CallingConvention.Default, 0u, retType, argType1, argType2, argType3);
	}

	public static MethodSig CreateStatic(TypeSig retType, params TypeSig[] argTypes)
	{
		return new MethodSig(CallingConvention.Default, 0u, retType, argTypes);
	}

	public static MethodSig CreateInstance(TypeSig retType)
	{
		return new MethodSig(CallingConvention.HasThis, 0u, retType);
	}

	public static MethodSig CreateInstance(TypeSig retType, TypeSig argType1)
	{
		return new MethodSig(CallingConvention.HasThis, 0u, retType, argType1);
	}

	public static MethodSig CreateInstance(TypeSig retType, TypeSig argType1, TypeSig argType2)
	{
		return new MethodSig(CallingConvention.HasThis, 0u, retType, argType1, argType2);
	}

	public static MethodSig CreateInstance(TypeSig retType, TypeSig argType1, TypeSig argType2, TypeSig argType3)
	{
		return new MethodSig(CallingConvention.HasThis, 0u, retType, argType1, argType2, argType3);
	}

	public static MethodSig CreateInstance(TypeSig retType, params TypeSig[] argTypes)
	{
		return new MethodSig(CallingConvention.HasThis, 0u, retType, argTypes);
	}

	public static MethodSig CreateStaticGeneric(uint genParamCount, TypeSig retType)
	{
		return new MethodSig(CallingConvention.Generic, genParamCount, retType);
	}

	public static MethodSig CreateStaticGeneric(uint genParamCount, TypeSig retType, TypeSig argType1)
	{
		return new MethodSig(CallingConvention.Generic, genParamCount, retType, argType1);
	}

	public static MethodSig CreateStaticGeneric(uint genParamCount, TypeSig retType, TypeSig argType1, TypeSig argType2)
	{
		return new MethodSig(CallingConvention.Generic, genParamCount, retType, argType1, argType2);
	}

	public static MethodSig CreateStaticGeneric(uint genParamCount, TypeSig retType, TypeSig argType1, TypeSig argType2, TypeSig argType3)
	{
		return new MethodSig(CallingConvention.Generic, genParamCount, retType, argType1, argType2, argType3);
	}

	public static MethodSig CreateStaticGeneric(uint genParamCount, TypeSig retType, params TypeSig[] argTypes)
	{
		return new MethodSig(CallingConvention.Generic, genParamCount, retType, argTypes);
	}

	public static MethodSig CreateInstanceGeneric(uint genParamCount, TypeSig retType)
	{
		return new MethodSig(CallingConvention.Generic | CallingConvention.HasThis, genParamCount, retType);
	}

	public static MethodSig CreateInstanceGeneric(uint genParamCount, TypeSig retType, TypeSig argType1)
	{
		return new MethodSig(CallingConvention.Generic | CallingConvention.HasThis, genParamCount, retType, argType1);
	}

	public static MethodSig CreateInstanceGeneric(uint genParamCount, TypeSig retType, TypeSig argType1, TypeSig argType2)
	{
		return new MethodSig(CallingConvention.Generic | CallingConvention.HasThis, genParamCount, retType, argType1, argType2);
	}

	public static MethodSig CreateInstanceGeneric(uint genParamCount, TypeSig retType, TypeSig argType1, TypeSig argType2, TypeSig argType3)
	{
		return new MethodSig(CallingConvention.Generic | CallingConvention.HasThis, genParamCount, retType, argType1, argType2, argType3);
	}

	public static MethodSig CreateInstanceGeneric(uint genParamCount, TypeSig retType, params TypeSig[] argTypes)
	{
		return new MethodSig(CallingConvention.Generic | CallingConvention.HasThis, genParamCount, retType, argTypes);
	}

	public MethodSig()
	{
		parameters = new List<TypeSig>();
	}

	public MethodSig(CallingConvention callingConvention)
	{
		base.callingConvention = callingConvention;
		parameters = new List<TypeSig>();
	}

	public MethodSig(CallingConvention callingConvention, uint genParamCount)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		parameters = new List<TypeSig>();
	}

	public MethodSig(CallingConvention callingConvention, uint genParamCount, TypeSig retType)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		base.retType = retType;
		parameters = new List<TypeSig>();
	}

	public MethodSig(CallingConvention callingConvention, uint genParamCount, TypeSig retType, TypeSig argType1)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		base.retType = retType;
		parameters = new List<TypeSig> { argType1 };
	}

	public MethodSig(CallingConvention callingConvention, uint genParamCount, TypeSig retType, TypeSig argType1, TypeSig argType2)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		base.retType = retType;
		parameters = new List<TypeSig> { argType1, argType2 };
	}

	public MethodSig(CallingConvention callingConvention, uint genParamCount, TypeSig retType, TypeSig argType1, TypeSig argType2, TypeSig argType3)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		base.retType = retType;
		parameters = new List<TypeSig> { argType1, argType2, argType3 };
	}

	public MethodSig(CallingConvention callingConvention, uint genParamCount, TypeSig retType, params TypeSig[] argTypes)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		base.retType = retType;
		parameters = new List<TypeSig>(argTypes);
	}

	public MethodSig(CallingConvention callingConvention, uint genParamCount, TypeSig retType, IList<TypeSig> argTypes)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		base.retType = retType;
		parameters = new List<TypeSig>(argTypes);
	}

	public MethodSig(CallingConvention callingConvention, uint genParamCount, TypeSig retType, IList<TypeSig> argTypes, IList<TypeSig> paramsAfterSentinel)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		base.retType = retType;
		parameters = new List<TypeSig>(argTypes);
		base.paramsAfterSentinel = ((paramsAfterSentinel == null) ? null : new List<TypeSig>(paramsAfterSentinel));
	}

	public MethodSig Clone()
	{
		return new MethodSig(callingConvention, genParamCount, retType, parameters, paramsAfterSentinel);
	}

	public override string ToString()
	{
		return FullNameFactory.MethodBaseSigFullName(this);
	}
}
