using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawEventPtrRow
{
	public readonly uint Event;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Event;
			}
			return 0u;
		}
	}

	public RawEventPtrRow(uint Event)
	{
		this.Event = Event;
	}
}
