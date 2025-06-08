namespace dnlib.DotNet.MD;

public readonly struct RawMethodPtrRow
{
	public readonly uint Method;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Method;
			}
			return 0u;
		}
	}

	public RawMethodPtrRow(uint Method)
	{
		this.Method = Method;
	}
}
