using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawFileRow
{
	public readonly uint Flags;

	public readonly uint Name;

	public readonly uint HashValue;

	public uint this[int index] => index switch
	{
		0 => Flags, 
		1 => Name, 
		2 => HashValue, 
		_ => 0u, 
	};

	public RawFileRow(uint Flags, uint Name, uint HashValue)
	{
		this.Flags = Flags;
		this.Name = Name;
		this.HashValue = HashValue;
	}
}
