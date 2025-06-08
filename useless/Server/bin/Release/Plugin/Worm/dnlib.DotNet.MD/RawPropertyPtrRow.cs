using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawPropertyPtrRow
{
	public readonly uint Property;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Property;
			}
			return 0u;
		}
	}

	public RawPropertyPtrRow(uint Property)
	{
		this.Property = Property;
	}
}
