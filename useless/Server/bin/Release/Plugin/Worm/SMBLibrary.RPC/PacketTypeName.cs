using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public enum PacketTypeName : byte
{
	Request = 0,
	Response = 2,
	Fault = 3,
	Bind = 11,
	BindAck = 12,
	BindNak = 13,
	AlterContext = 14,
	AlterContextResponse = 15,
	Shutdown = 17,
	COCancel = 18,
	Orphaned = 19
}
