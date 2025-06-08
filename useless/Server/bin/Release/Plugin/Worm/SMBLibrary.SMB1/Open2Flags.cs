using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum Open2Flags : ushort
{
	REQ_ATTRIB = 1,
	REQ_OPLOCK = 2,
	REQ_OPBATCH = 4,
	REQ_EASIZE = 8
}
