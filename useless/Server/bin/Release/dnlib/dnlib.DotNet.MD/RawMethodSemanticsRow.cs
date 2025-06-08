namespace dnlib.DotNet.MD;

public readonly struct RawMethodSemanticsRow
{
	public readonly ushort Semantic;

	public readonly uint Method;

	public readonly uint Association;

	public uint this[int index] => index switch
	{
		0 => Semantic, 
		1 => Method, 
		2 => Association, 
		_ => 0u, 
	};

	public RawMethodSemanticsRow(ushort Semantic, uint Method, uint Association)
	{
		this.Semantic = Semantic;
		this.Method = Method;
		this.Association = Association;
	}
}
