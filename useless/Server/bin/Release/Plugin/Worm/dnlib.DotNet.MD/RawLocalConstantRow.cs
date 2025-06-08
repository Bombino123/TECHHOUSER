using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawLocalConstantRow
{
	public readonly uint Name;

	public readonly uint Signature;

	public uint this[int index] => index switch
	{
		0 => Name, 
		1 => Signature, 
		_ => 0u, 
	};

	public RawLocalConstantRow(uint Name, uint Signature)
	{
		this.Name = Name;
		this.Signature = Signature;
	}
}
