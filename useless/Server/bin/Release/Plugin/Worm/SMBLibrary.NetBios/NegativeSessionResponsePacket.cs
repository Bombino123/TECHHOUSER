using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NegativeSessionResponsePacket : SessionPacket
{
	public byte ErrorCode;

	public override int Length => 5;

	public NegativeSessionResponsePacket()
	{
		Type = SessionPacketTypeName.NegativeSessionResponse;
	}

	public NegativeSessionResponsePacket(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		ErrorCode = ByteReader.ReadByte(Trailer, offset);
	}

	public override byte[] GetBytes()
	{
		Trailer = new byte[1];
		Trailer[0] = ErrorCode;
		return base.GetBytes();
	}
}
