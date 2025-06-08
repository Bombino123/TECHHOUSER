namespace dnlib.DotNet.MD;

public readonly struct RawCustomDebugInformationRow
{
	public readonly uint Parent;

	public readonly uint Kind;

	public readonly uint Value;

	public uint this[int index] => index switch
	{
		0 => Parent, 
		1 => Kind, 
		2 => Value, 
		_ => 0u, 
	};

	public RawCustomDebugInformationRow(uint Parent, uint Kind, uint Value)
	{
		this.Parent = Parent;
		this.Kind = Kind;
		this.Value = Value;
	}
}
