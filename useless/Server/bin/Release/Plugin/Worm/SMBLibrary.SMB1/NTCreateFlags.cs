using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum NTCreateFlags : uint
{
	NT_CREATE_REQUEST_OPLOCK = 2u,
	NT_CREATE_REQUEST_OPBATCH = 4u,
	NT_CREATE_OPEN_TARGET_DIR = 8u,
	NT_CREATE_REQUEST_EXTENDED_RESPONSE = 0x10u
}
