namespace dnlib.DotNet;

public sealed class FieldSig : CallingConventionSig
{
	private TypeSig type;

	public TypeSig Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public FieldSig()
	{
		callingConvention = CallingConvention.Field;
	}

	public FieldSig(TypeSig type)
	{
		callingConvention = CallingConvention.Field;
		this.type = type;
	}

	internal FieldSig(CallingConvention callingConvention, TypeSig type)
	{
		base.callingConvention = callingConvention;
		this.type = type;
	}

	public FieldSig Clone()
	{
		return new FieldSig(callingConvention, type);
	}

	public override string ToString()
	{
		return FullNameFactory.FullName(type, isReflection: false);
	}
}
