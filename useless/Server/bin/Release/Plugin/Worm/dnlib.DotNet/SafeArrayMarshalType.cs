using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class SafeArrayMarshalType : MarshalType
{
	private VariantType vt;

	private ITypeDefOrRef userDefinedSubType;

	public VariantType VariantType
	{
		get
		{
			return vt;
		}
		set
		{
			vt = value;
		}
	}

	public ITypeDefOrRef UserDefinedSubType
	{
		get
		{
			return userDefinedSubType;
		}
		set
		{
			userDefinedSubType = value;
		}
	}

	public bool IsVariantTypeValid => vt != VariantType.NotInitialized;

	public bool IsUserDefinedSubTypeValid => userDefinedSubType != null;

	public SafeArrayMarshalType()
		: this(VariantType.NotInitialized, null)
	{
	}

	public SafeArrayMarshalType(VariantType vt)
		: this(vt, null)
	{
	}

	public SafeArrayMarshalType(ITypeDefOrRef userDefinedSubType)
		: this(VariantType.NotInitialized, userDefinedSubType)
	{
	}

	public SafeArrayMarshalType(VariantType vt, ITypeDefOrRef userDefinedSubType)
		: base(NativeType.SafeArray)
	{
		this.vt = vt;
		this.userDefinedSubType = userDefinedSubType;
	}

	public override string ToString()
	{
		ITypeDefOrRef typeDefOrRef = userDefinedSubType;
		if (typeDefOrRef != null)
		{
			return $"{nativeType} ({vt}, {typeDefOrRef})";
		}
		return $"{nativeType} ({vt})";
	}
}
