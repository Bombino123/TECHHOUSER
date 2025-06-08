using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum SecurityMode : byte
{
	UserSecurityMode = 1,
	EncryptPasswords = 2,
	SecuritySignaturesEnabled = 4,
	SecuritySignaturesRequired = 8
}
