using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawParamRow
{
	public readonly ushort Flags;

	public readonly ushort Sequence;

	public readonly uint Name;

	public uint this[int index] => index switch
	{
		0 => Flags, 
		1 => Sequence, 
		2 => Name, 
		_ => 0u, 
	};

	public RawParamRow(ushort Flags, ushort Sequence, uint Name)
	{
		this.Flags = Flags;
		this.Sequence = Sequence;
		this.Name = Name;
	}
}
