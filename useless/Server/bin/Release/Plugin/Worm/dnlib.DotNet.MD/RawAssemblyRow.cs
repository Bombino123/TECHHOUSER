using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawAssemblyRow
{
	public readonly uint HashAlgId;

	public readonly ushort MajorVersion;

	public readonly ushort MinorVersion;

	public readonly ushort BuildNumber;

	public readonly ushort RevisionNumber;

	public readonly uint Flags;

	public readonly uint PublicKey;

	public readonly uint Name;

	public readonly uint Locale;

	public uint this[int index] => index switch
	{
		0 => HashAlgId, 
		1 => MajorVersion, 
		2 => MinorVersion, 
		3 => BuildNumber, 
		4 => RevisionNumber, 
		5 => Flags, 
		6 => PublicKey, 
		7 => Name, 
		8 => Locale, 
		_ => 0u, 
	};

	public RawAssemblyRow(uint HashAlgId, ushort MajorVersion, ushort MinorVersion, ushort BuildNumber, ushort RevisionNumber, uint Flags, uint PublicKey, uint Name, uint Locale)
	{
		this.HashAlgId = HashAlgId;
		this.MajorVersion = MajorVersion;
		this.MinorVersion = MinorVersion;
		this.BuildNumber = BuildNumber;
		this.RevisionNumber = RevisionNumber;
		this.Flags = Flags;
		this.PublicKey = PublicKey;
		this.Name = Name;
		this.Locale = Locale;
	}
}
