namespace dnlib.DotNet.MD;

public readonly struct RawEventRow
{
	public readonly ushort EventFlags;

	public readonly uint Name;

	public readonly uint EventType;

	public uint this[int index] => index switch
	{
		0 => EventFlags, 
		1 => Name, 
		2 => EventType, 
		_ => 0u, 
	};

	public RawEventRow(ushort EventFlags, uint Name, uint EventType)
	{
		this.EventFlags = EventFlags;
		this.Name = Name;
		this.EventType = EventType;
	}
}
