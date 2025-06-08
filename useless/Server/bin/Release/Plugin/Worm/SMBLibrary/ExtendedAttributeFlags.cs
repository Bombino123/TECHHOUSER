using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[Flags]
[ComVisible(true)]
public enum ExtendedAttributeFlags : byte
{
	FILE_NEED_EA = 0x80
}
