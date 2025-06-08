namespace dnlib.DotNet.MD;

public readonly struct RawParamPtrRow
{
	public readonly uint Param;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Param;
			}
			return 0u;
		}
	}

	public RawParamPtrRow(uint Param)
	{
		this.Param = Param;
	}
}
