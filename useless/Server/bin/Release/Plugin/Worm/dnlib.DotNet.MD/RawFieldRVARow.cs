using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawFieldRVARow
{
	public readonly uint RVA;

	public readonly uint Field;

	public uint this[int index] => index switch
	{
		0 => RVA, 
		1 => Field, 
		_ => 0u, 
	};

	public RawFieldRVARow(uint RVA, uint Field)
	{
		this.RVA = RVA;
		this.Field = Field;
	}
}
