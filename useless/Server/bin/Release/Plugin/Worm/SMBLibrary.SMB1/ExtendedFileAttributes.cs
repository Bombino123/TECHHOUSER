using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum ExtendedFileAttributes : uint
{
	ReadOnly = 1u,
	Hidden = 2u,
	System = 4u,
	Directory = 0x10u,
	Archive = 0x20u,
	Normal = 0x80u,
	Temporary = 0x100u,
	Sparse = 0x200u,
	ReparsePoint = 0x400u,
	Compressed = 0x800u,
	Offline = 0x1000u,
	NotIndexed = 0x2000u,
	Encrypted = 0x4000u,
	PosixSemantics = 0x1000000u,
	BackupSemantics = 0x2000000u,
	DeleteOnClose = 0x4000000u,
	SequentialScan = 0x8000000u,
	RandomAccess = 0x10000000u,
	NoBuffering = 0x10000000u,
	WriteThrough = 0x80000000u
}
