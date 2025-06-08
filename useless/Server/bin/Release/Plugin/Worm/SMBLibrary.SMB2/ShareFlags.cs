using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum ShareFlags : uint
{
	ManualCaching = 0u,
	AutoCaching = 0x10u,
	VdoCaching = 0x20u,
	NoCaching = 0x30u,
	Dfs = 1u,
	DfsRoot = 2u,
	RestrictExclusiveOpens = 0x100u,
	ForceSharedDelete = 0x200u,
	AllowNamespaceCaching = 0x400u,
	AccessBasedDirectoryEnum = 0x800u,
	ForceLevel2Oplock = 0x1000u,
	EnableHashV1 = 0x2000u,
	EnableHashV2 = 0x4000u,
	EncryptData = 0x8000u
}
