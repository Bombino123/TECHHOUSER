using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
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
