using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum SMB2PacketHeaderFlags : uint
{
	ServerToRedir = 1u,
	AsyncCommand = 2u,
	RelatedOperations = 4u,
	Signed = 8u,
	DfsOperations = 0x10000000u
}
