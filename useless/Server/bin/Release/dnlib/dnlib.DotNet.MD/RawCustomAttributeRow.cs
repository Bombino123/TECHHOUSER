namespace dnlib.DotNet.MD;

public readonly struct RawCustomAttributeRow
{
	public readonly uint Parent;

	public readonly uint Type;

	public readonly uint Value;

	public uint this[int index] => index switch
	{
		0 => Parent, 
		1 => Type, 
		2 => Value, 
		_ => 0u, 
	};

	public RawCustomAttributeRow(uint Parent, uint Type, uint Value)
	{
		this.Parent = Parent;
		this.Type = Type;
		this.Value = Value;
	}
}
