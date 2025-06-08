namespace dnlib.DotNet.MD;

public readonly struct RawConstantRow
{
	public readonly byte Type;

	public readonly byte Padding;

	public readonly uint Parent;

	public readonly uint Value;

	public uint this[int index] => index switch
	{
		0 => Type, 
		1 => Padding, 
		2 => Parent, 
		3 => Value, 
		_ => 0u, 
	};

	public RawConstantRow(byte Type, byte Padding, uint Parent, uint Value)
	{
		this.Type = Type;
		this.Padding = Padding;
		this.Parent = Parent;
		this.Value = Value;
	}
}
