namespace dnlib.DotNet.MD;

public readonly struct RawFieldLayoutRow
{
	public readonly uint OffSet;

	public readonly uint Field;

	public uint this[int index] => index switch
	{
		0 => OffSet, 
		1 => Field, 
		_ => 0u, 
	};

	public RawFieldLayoutRow(uint OffSet, uint Field)
	{
		this.OffSet = OffSet;
		this.Field = Field;
	}
}
