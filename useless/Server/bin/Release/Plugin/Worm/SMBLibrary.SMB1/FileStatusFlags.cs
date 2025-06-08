using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum FileStatusFlags : ushort
{
	NO_EAS = 1,
	NO_SUBSTREAMS = 2,
	NO_REPARSETAG = 4
}
