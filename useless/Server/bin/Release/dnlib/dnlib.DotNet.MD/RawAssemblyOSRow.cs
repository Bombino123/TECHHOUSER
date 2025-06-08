namespace dnlib.DotNet.MD;

public readonly struct RawAssemblyOSRow
{
	public readonly uint OSPlatformId;

	public readonly uint OSMajorVersion;

	public readonly uint OSMinorVersion;

	public uint this[int index] => index switch
	{
		0 => OSPlatformId, 
		1 => OSMajorVersion, 
		2 => OSMinorVersion, 
		_ => 0u, 
	};

	public RawAssemblyOSRow(uint OSPlatformId, uint OSMajorVersion, uint OSMinorVersion)
	{
		this.OSPlatformId = OSPlatformId;
		this.OSMajorVersion = OSMajorVersion;
		this.OSMinorVersion = OSMinorVersion;
	}
}
