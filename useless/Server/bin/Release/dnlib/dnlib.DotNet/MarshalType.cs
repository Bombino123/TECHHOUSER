namespace dnlib.DotNet;

public class MarshalType
{
	protected readonly NativeType nativeType;

	public NativeType NativeType => nativeType;

	public MarshalType(NativeType nativeType)
	{
		this.nativeType = nativeType;
	}

	public override string ToString()
	{
		return nativeType.ToString();
	}
}
