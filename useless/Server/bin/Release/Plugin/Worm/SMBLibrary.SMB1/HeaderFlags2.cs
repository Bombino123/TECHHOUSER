using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum HeaderFlags2 : ushort
{
	LongNamesAllowed = 1,
	ExtendedAttributes = 2,
	SecuritySignature = 4,
	CompressedData = 8,
	SecuritySignatureRequired = 0x10,
	LongNameUsed = 0x40,
	ReparsePath = 0x400,
	ExtendedSecurity = 0x800,
	DFS = 0x1000,
	ReadIfExecute = 0x2000,
	NTStatusCode = 0x4000,
	Unicode = 0x8000
}
