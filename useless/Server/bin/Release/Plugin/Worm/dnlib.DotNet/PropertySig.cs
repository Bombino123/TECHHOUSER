using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class PropertySig : MethodBaseSig
{
	public static PropertySig CreateStatic(TypeSig retType)
	{
		return new PropertySig(hasThis: false, retType);
	}

	public static PropertySig CreateStatic(TypeSig retType, TypeSig argType1)
	{
		return new PropertySig(hasThis: false, retType, argType1);
	}

	public static PropertySig CreateStatic(TypeSig retType, TypeSig argType1, TypeSig argType2)
	{
		return new PropertySig(hasThis: false, retType, argType1, argType2);
	}

	public static PropertySig CreateStatic(TypeSig retType, TypeSig argType1, TypeSig argType2, TypeSig argType3)
	{
		return new PropertySig(hasThis: false, retType, argType1, argType2, argType3);
	}

	public static PropertySig CreateStatic(TypeSig retType, params TypeSig[] argTypes)
	{
		return new PropertySig(hasThis: false, retType, argTypes);
	}

	public static PropertySig CreateInstance(TypeSig retType)
	{
		return new PropertySig(hasThis: true, retType);
	}

	public static PropertySig CreateInstance(TypeSig retType, TypeSig argType1)
	{
		return new PropertySig(hasThis: true, retType, argType1);
	}

	public static PropertySig CreateInstance(TypeSig retType, TypeSig argType1, TypeSig argType2)
	{
		return new PropertySig(hasThis: true, retType, argType1, argType2);
	}

	public static PropertySig CreateInstance(TypeSig retType, TypeSig argType1, TypeSig argType2, TypeSig argType3)
	{
		return new PropertySig(hasThis: true, retType, argType1, argType2, argType3);
	}

	public static PropertySig CreateInstance(TypeSig retType, params TypeSig[] argTypes)
	{
		return new PropertySig(hasThis: true, retType, argTypes);
	}

	public PropertySig()
	{
		callingConvention = CallingConvention.Property;
		parameters = new List<TypeSig>();
	}

	internal PropertySig(CallingConvention callingConvention)
	{
		base.callingConvention = callingConvention;
		parameters = new List<TypeSig>();
	}

	public PropertySig(bool hasThis)
	{
		callingConvention = CallingConvention.Property | (hasThis ? CallingConvention.HasThis : CallingConvention.Default);
		parameters = new List<TypeSig>();
	}

	public PropertySig(bool hasThis, TypeSig retType)
	{
		callingConvention = CallingConvention.Property | (hasThis ? CallingConvention.HasThis : CallingConvention.Default);
		base.retType = retType;
		parameters = new List<TypeSig>();
	}

	public PropertySig(bool hasThis, TypeSig retType, TypeSig argType1)
	{
		callingConvention = CallingConvention.Property | (hasThis ? CallingConvention.HasThis : CallingConvention.Default);
		base.retType = retType;
		parameters = new List<TypeSig> { argType1 };
	}

	public PropertySig(bool hasThis, TypeSig retType, TypeSig argType1, TypeSig argType2)
	{
		callingConvention = CallingConvention.Property | (hasThis ? CallingConvention.HasThis : CallingConvention.Default);
		base.retType = retType;
		parameters = new List<TypeSig> { argType1, argType2 };
	}

	public PropertySig(bool hasThis, TypeSig retType, TypeSig argType1, TypeSig argType2, TypeSig argType3)
	{
		callingConvention = CallingConvention.Property | (hasThis ? CallingConvention.HasThis : CallingConvention.Default);
		base.retType = retType;
		parameters = new List<TypeSig> { argType1, argType2, argType3 };
	}

	public PropertySig(bool hasThis, TypeSig retType, params TypeSig[] argTypes)
	{
		callingConvention = CallingConvention.Property | (hasThis ? CallingConvention.HasThis : CallingConvention.Default);
		base.retType = retType;
		parameters = new List<TypeSig>(argTypes);
	}

	internal PropertySig(CallingConvention callingConvention, uint genParamCount, TypeSig retType, IList<TypeSig> argTypes, IList<TypeSig> paramsAfterSentinel)
	{
		base.callingConvention = callingConvention;
		base.genParamCount = genParamCount;
		base.retType = retType;
		parameters = new List<TypeSig>(argTypes);
		base.paramsAfterSentinel = ((paramsAfterSentinel == null) ? null : new List<TypeSig>(paramsAfterSentinel));
	}

	public PropertySig Clone()
	{
		return new PropertySig(callingConvention, genParamCount, retType, parameters, paramsAfterSentinel);
	}

	public override string ToString()
	{
		return FullNameFactory.MethodBaseSigFullName(this);
	}
}
