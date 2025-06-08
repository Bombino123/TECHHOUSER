using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawENCMapRow
{
	public readonly uint Token;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Token;
			}
			return 0u;
		}
	}

	public RawENCMapRow(uint Token)
	{
		this.Token = Token;
	}
}
