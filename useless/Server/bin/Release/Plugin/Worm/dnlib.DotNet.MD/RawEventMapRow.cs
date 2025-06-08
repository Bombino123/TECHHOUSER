using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawEventMapRow
{
	public readonly uint Parent;

	public readonly uint EventList;

	public uint this[int index] => index switch
	{
		0 => Parent, 
		1 => EventList, 
		_ => 0u, 
	};

	public RawEventMapRow(uint Parent, uint EventList)
	{
		this.Parent = Parent;
		this.EventList = EventList;
	}
}
