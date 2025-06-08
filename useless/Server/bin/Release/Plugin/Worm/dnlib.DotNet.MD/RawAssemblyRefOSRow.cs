using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawAssemblyRefOSRow
{
	public readonly uint OSPlatformId;

	public readonly uint OSMajorVersion;

	public readonly uint OSMinorVersion;

	public readonly uint AssemblyRef;

	public uint this[int index] => index switch
	{
		0 => OSPlatformId, 
		1 => OSMajorVersion, 
		2 => OSMinorVersion, 
		3 => AssemblyRef, 
		_ => 0u, 
	};

	public RawAssemblyRefOSRow(uint OSPlatformId, uint OSMajorVersion, uint OSMinorVersion, uint AssemblyRef)
	{
		this.OSPlatformId = OSPlatformId;
		this.OSMajorVersion = OSMajorVersion;
		this.OSMinorVersion = OSMinorVersion;
		this.AssemblyRef = AssemblyRef;
	}
}
