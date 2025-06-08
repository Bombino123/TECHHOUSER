using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum TreeConnectFlags : ushort
{
	DisconnectTID = 1,
	ExtendedSignatures = 4,
	ExtendedResponse = 8
}
