namespace dnlib.DotNet.MD;

public readonly struct RawLocalVariableRow
{
	public readonly ushort Attributes;

	public readonly ushort Index;

	public readonly uint Name;

	public uint this[int index] => index switch
	{
		0 => Attributes, 
		1 => Index, 
		2 => Name, 
		_ => 0u, 
	};

	public RawLocalVariableRow(ushort Attributes, ushort Index, uint Name)
	{
		this.Attributes = Attributes;
		this.Index = Index;
		this.Name = Name;
	}
}
