using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class SessionRetargetResponsePacket : SessionPacket
{
	private uint IPAddress;

	private ushort Port;

	public override int Length => 10;

	public SessionRetargetResponsePacket()
	{
		Type = SessionPacketTypeName.RetargetSessionResponse;
	}

	public SessionRetargetResponsePacket(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		IPAddress = BigEndianConverter.ToUInt32(Trailer, offset);
		Port = BigEndianConverter.ToUInt16(Trailer, offset + 4);
	}

	public override byte[] GetBytes()
	{
		Trailer = new byte[6];
		BigEndianWriter.WriteUInt32(Trailer, 0, IPAddress);
		BigEndianWriter.WriteUInt16(Trailer, 4, Port);
		return base.GetBytes();
	}
}
