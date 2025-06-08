namespace dnlib.DotNet.MD;

public readonly struct RawTypeSpecRow
{
	public readonly uint Signature;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Signature;
			}
			return 0u;
		}
	}

	public RawTypeSpecRow(uint Signature)
	{
		this.Signature = Signature;
	}
}
