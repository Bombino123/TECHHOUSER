using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class SessionRequestPacket : SessionPacket
{
	public string CalledName;

	public string CallingName;

	public override int Length
	{
		get
		{
			byte[] array = NetBiosUtils.EncodeName(CalledName, string.Empty);
			byte[] array2 = NetBiosUtils.EncodeName(CallingName, string.Empty);
			return 4 + array.Length + array2.Length;
		}
	}

	public SessionRequestPacket()
	{
		Type = SessionPacketTypeName.SessionRequest;
	}

	public SessionRequestPacket(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		CalledName = NetBiosUtils.DecodeName(Trailer, ref offset);
		CallingName = NetBiosUtils.DecodeName(Trailer, ref offset);
	}

	public override byte[] GetBytes()
	{
		byte[] array = NetBiosUtils.EncodeName(CalledName, string.Empty);
		byte[] array2 = NetBiosUtils.EncodeName(CallingName, string.Empty);
		Trailer = new byte[array.Length + array2.Length];
		ByteWriter.WriteBytes(Trailer, 0, array);
		ByteWriter.WriteBytes(Trailer, array.Length, array2);
		return base.GetBytes();
	}
}
