using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public enum SessionPacketTypeName : byte
{
	SessionMessage = 0,
	SessionRequest = 129,
	PositiveSessionResponse = 130,
	NegativeSessionResponse = 131,
	RetargetSessionResponse = 132,
	SessionKeepAlive = 133
}
