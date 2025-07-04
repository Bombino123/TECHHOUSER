using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawModuleRefRow
{
	public readonly uint Name;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Name;
			}
			return 0u;
		}
	}

	public RawModuleRefRow(uint Name)
	{
		this.Name = Name;
	}
}
