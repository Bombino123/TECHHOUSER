using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawFieldPtrRow
{
	public readonly uint Field;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Field;
			}
			return 0u;
		}
	}

	public RawFieldPtrRow(uint Field)
	{
		this.Field = Field;
	}
}
