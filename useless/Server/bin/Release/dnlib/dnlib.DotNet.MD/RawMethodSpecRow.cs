namespace dnlib.DotNet.MD;

public readonly struct RawMethodSpecRow
{
	public readonly uint Method;

	public readonly uint Instantiation;

	public uint this[int index] => index switch
	{
		0 => Method, 
		1 => Instantiation, 
		_ => 0u, 
	};

	public RawMethodSpecRow(uint Method, uint Instantiation)
	{
		this.Method = Method;
		this.Instantiation = Instantiation;
	}
}
