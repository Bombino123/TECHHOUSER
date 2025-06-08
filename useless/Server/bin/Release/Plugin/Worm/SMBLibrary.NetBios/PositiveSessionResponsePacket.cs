using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class PositiveSessionResponsePacket : SessionPacket
{
	public override int Length => 4;

	public PositiveSessionResponsePacket()
	{
		Type = SessionPacketTypeName.PositiveSessionResponse;
	}

	public PositiveSessionResponsePacket(byte[] buffer, int offset)
		: base(buffer, offset)
	{
	}

	public override byte[] GetBytes()
	{
		Trailer = new byte[0];
		return base.GetBytes();
	}
}
