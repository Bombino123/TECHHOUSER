using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[Flags]
[ComVisible(true)]
public enum PacketFlags : byte
{
	FirstFragment = 1,
	LastFragment = 2,
	PendingCancel = 4,
	ConcurrntMultiplexing = 0x10,
	DidNotExecute = 0x20,
	Maybe = 0x40,
	ObjectUUID = 0x80
}
