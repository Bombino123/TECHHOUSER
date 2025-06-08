namespace dnlib.DotNet.MD;

public readonly struct RawStandAloneSigRow
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

	public RawStandAloneSigRow(uint Signature)
	{
		this.Signature = Signature;
	}
}
