using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum Capabilities : uint
{
	DFS = 1u,
	Leasing = 2u,
	LargeMTU = 4u,
	MultiChannel = 8u,
	PersistentHandles = 0x10u,
	DirectoryLeasing = 0x20u,
	Encryption = 0x40u
}
