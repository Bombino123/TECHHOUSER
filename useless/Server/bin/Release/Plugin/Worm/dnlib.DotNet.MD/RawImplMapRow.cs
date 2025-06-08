using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawImplMapRow
{
	public readonly ushort MappingFlags;

	public readonly uint MemberForwarded;

	public readonly uint ImportName;

	public readonly uint ImportScope;

	public uint this[int index] => index switch
	{
		0 => MappingFlags, 
		1 => MemberForwarded, 
		2 => ImportName, 
		3 => ImportScope, 
		_ => 0u, 
	};

	public RawImplMapRow(ushort MappingFlags, uint MemberForwarded, uint ImportName, uint ImportScope)
	{
		this.MappingFlags = MappingFlags;
		this.MemberForwarded = MemberForwarded;
		this.ImportName = ImportName;
		this.ImportScope = ImportScope;
	}
}
