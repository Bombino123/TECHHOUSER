using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawAssemblyRefRow
{
	public readonly ushort MajorVersion;

	public readonly ushort MinorVersion;

	public readonly ushort BuildNumber;

	public readonly ushort RevisionNumber;

	public readonly uint Flags;

	public readonly uint PublicKeyOrToken;

	public readonly uint Name;

	public readonly uint Locale;

	public readonly uint HashValue;

	public uint this[int index] => index switch
	{
		0 => MajorVersion, 
		1 => MinorVersion, 
		2 => BuildNumber, 
		3 => RevisionNumber, 
		4 => Flags, 
		5 => PublicKeyOrToken, 
		6 => Name, 
		7 => Locale, 
		8 => HashValue, 
		_ => 0u, 
	};

	public RawAssemblyRefRow(ushort MajorVersion, ushort MinorVersion, ushort BuildNumber, ushort RevisionNumber, uint Flags, uint PublicKeyOrToken, uint Name, uint Locale, uint HashValue)
	{
		this.MajorVersion = MajorVersion;
		this.MinorVersion = MinorVersion;
		this.BuildNumber = BuildNumber;
		this.RevisionNumber = RevisionNumber;
		this.Flags = Flags;
		this.PublicKeyOrToken = PublicKeyOrToken;
		this.Name = Name;
		this.Locale = Locale;
		this.HashValue = HashValue;
	}
}
