using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawPropertyRow
{
	public readonly ushort PropFlags;

	public readonly uint Name;

	public readonly uint Type;

	public uint this[int index] => index switch
	{
		0 => PropFlags, 
		1 => Name, 
		2 => Type, 
		_ => 0u, 
	};

	public RawPropertyRow(ushort PropFlags, uint Name, uint Type)
	{
		this.PropFlags = PropFlags;
		this.Name = Name;
		this.Type = Type;
	}
}
