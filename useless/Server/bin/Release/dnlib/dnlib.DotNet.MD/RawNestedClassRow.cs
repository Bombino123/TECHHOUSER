namespace dnlib.DotNet.MD;

public readonly struct RawNestedClassRow
{
	public readonly uint NestedClass;

	public readonly uint EnclosingClass;

	public uint this[int index] => index switch
	{
		0 => NestedClass, 
		1 => EnclosingClass, 
		_ => 0u, 
	};

	public RawNestedClassRow(uint NestedClass, uint EnclosingClass)
	{
		this.NestedClass = NestedClass;
		this.EnclosingClass = EnclosingClass;
	}
}
