namespace dnlib.DotNet.MD;

public readonly struct RawMethodImplRow
{
	public readonly uint Class;

	public readonly uint MethodBody;

	public readonly uint MethodDeclaration;

	public uint this[int index] => index switch
	{
		0 => Class, 
		1 => MethodBody, 
		2 => MethodDeclaration, 
		_ => 0u, 
	};

	public RawMethodImplRow(uint Class, uint MethodBody, uint MethodDeclaration)
	{
		this.Class = Class;
		this.MethodBody = MethodBody;
		this.MethodDeclaration = MethodDeclaration;
	}
}
