using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum ShareCapabilities : uint
{
	Dfs = 8u,
	ContinuousAvailability = 0x10u,
	Scaleout = 0x20u,
	Cluster = 0x40u,
	Asymmetric = 0x80u
}
