using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[Flags]
[ComVisible(true)]
public enum FileSystemAttributes : uint
{
	CaseSensitiveSearch = 1u,
	CasePreservedNamed = 2u,
	UnicodeOnDisk = 4u,
	PersistentACLs = 8u,
	FileCompression = 0x10u,
	VolumeQuotas = 0x20u,
	SupportsSparseFiles = 0x40u,
	SupportsReparsePoints = 0x80u,
	SupportsRemoteStorage = 0x100u,
	VolumeIsCompressed = 0x8000u,
	SupportsObjectIDs = 0x10000u,
	SupportsEncryption = 0x20000u,
	NamedStreams = 0x40000u,
	ReadOnlyVolume = 0x80000u,
	SequentialWriteOnce = 0x100000u,
	SupportsTransactions = 0x200000u,
	SupportsHardLinks = 0x400000u,
	SupportsExtendedAttributes = 0x800000u,
	SupportsOpenByFileID = 0x1000000u,
	SupportsUSNJournal = 0x2000000u
}
