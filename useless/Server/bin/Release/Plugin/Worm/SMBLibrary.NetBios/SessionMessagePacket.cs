using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class SessionMessagePacket : SessionPacket
{
	public SessionMessagePacket()
	{
		Type = SessionPacketTypeName.SessionMessage;
	}

	public SessionMessagePacket(byte[] buffer, int offset)
		: base(buffer, offset)
	{
	}
}
