using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
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
