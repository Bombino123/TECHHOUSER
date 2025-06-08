using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum TransactionFlags : ushort
{
	DISCONNECT_TID = 1,
	NO_RESPONSE = 2
}
