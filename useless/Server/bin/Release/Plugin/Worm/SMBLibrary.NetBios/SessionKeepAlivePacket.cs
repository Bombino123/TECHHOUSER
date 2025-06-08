using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class SessionKeepAlivePacket : SessionPacket
{
	public override int Length => 4;

	public SessionKeepAlivePacket()
	{
		Type = SessionPacketTypeName.SessionKeepAlive;
	}

	public SessionKeepAlivePacket(byte[] buffer, int offset)
		: base(buffer, offset)
	{
	}

	public override byte[] GetBytes()
	{
		Trailer = new byte[0];
		return base.GetBytes();
	}
}
