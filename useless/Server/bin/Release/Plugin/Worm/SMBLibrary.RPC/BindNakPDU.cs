using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class BindNakPDU : RPCPDU
{
	public const int BindNakFieldsFixedLength = 2;

	public RejectionReason RejectReason;

	public VersionsSupported Versions;

	public override int Length
	{
		get
		{
			int num = 18;
			if (Versions != null)
			{
				num += Versions.Length;
			}
			return num;
		}
	}

	public BindNakPDU()
	{
		PacketType = PacketTypeName.BindNak;
	}

	public BindNakPDU(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		offset += 16;
		RejectReason = (RejectionReason)LittleEndianReader.ReadUInt16(buffer, ref offset);
		Versions = new VersionsSupported(buffer, offset);
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		WriteCommonFieldsBytes(array);
		int offset = 16;
		LittleEndianWriter.WriteUInt16(array, ref offset, (ushort)RejectReason);
		Versions.WriteBytes(array, offset);
		return array;
	}
}
