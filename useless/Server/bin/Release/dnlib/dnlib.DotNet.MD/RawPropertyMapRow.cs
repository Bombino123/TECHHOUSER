namespace dnlib.DotNet.MD;

public readonly struct RawPropertyMapRow
{
	public readonly uint Parent;

	public readonly uint PropertyList;

	public uint this[int index] => index switch
	{
		0 => Parent, 
		1 => PropertyList, 
		_ => 0u, 
	};

	public RawPropertyMapRow(uint Parent, uint PropertyList)
	{
		this.Parent = Parent;
		this.PropertyList = PropertyList;
	}
}
