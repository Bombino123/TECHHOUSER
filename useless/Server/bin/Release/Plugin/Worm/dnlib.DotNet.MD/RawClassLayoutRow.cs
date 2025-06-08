using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawClassLayoutRow
{
	public readonly ushort PackingSize;

	public readonly uint ClassSize;

	public readonly uint Parent;

	public uint this[int index] => index switch
	{
		0 => PackingSize, 
		1 => ClassSize, 
		2 => Parent, 
		_ => 0u, 
	};

	public RawClassLayoutRow(ushort PackingSize, uint ClassSize, uint Parent)
	{
		this.PackingSize = PackingSize;
		this.ClassSize = ClassSize;
		this.Parent = Parent;
	}
}
