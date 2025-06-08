using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawManifestResourceRow
{
	public readonly uint Offset;

	public readonly uint Flags;

	public readonly uint Name;

	public readonly uint Implementation;

	public uint this[int index] => index switch
	{
		0 => Offset, 
		1 => Flags, 
		2 => Name, 
		3 => Implementation, 
		_ => 0u, 
	};

	public RawManifestResourceRow(uint Offset, uint Flags, uint Name, uint Implementation)
	{
		this.Offset = Offset;
		this.Flags = Flags;
		this.Name = Name;
		this.Implementation = Implementation;
	}
}
