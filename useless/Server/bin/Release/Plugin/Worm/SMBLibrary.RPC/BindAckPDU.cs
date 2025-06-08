using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class BindAckPDU : RPCPDU
{
	public const int BindAckFieldsFixedLength = 8;

	public ushort MaxTransmitFragmentSize;

	public ushort MaxReceiveFragmentSize;

	public uint AssociationGroupID;

	public string SecondaryAddress;

	public ResultList ResultList;

	public byte[] AuthVerifier;

	public override int Length
	{
		get
		{
			int num = (4 - (SecondaryAddress.Length + 3) % 4) % 4;
			return 24 + SecondaryAddress.Length + 3 + num + ResultList.Length + AuthLength;
		}
	}

	public BindAckPDU()
	{
		PacketType = PacketTypeName.BindAck;
		SecondaryAddress = string.Empty;
		ResultList = new ResultList();
		AuthVerifier = new byte[0];
	}

	public BindAckPDU(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		int num = offset;
		offset += 16;
		MaxTransmitFragmentSize = LittleEndianReader.ReadUInt16(buffer, ref offset);
		MaxReceiveFragmentSize = LittleEndianReader.ReadUInt16(buffer, ref offset);
		AssociationGroupID = LittleEndianReader.ReadUInt32(buffer, ref offset);
		SecondaryAddress = RPCHelper.ReadPortAddress(buffer, ref offset);
		int num2 = (4 - (offset - num) % 4) % 4;
		offset += num2;
		ResultList = new ResultList(buffer, offset);
		offset += ResultList.Length;
		AuthVerifier = ByteReader.ReadBytes(buffer, offset, AuthLength);
	}

	public override byte[] GetBytes()
	{
		AuthLength = (ushort)AuthVerifier.Length;
		int num = (4 - (SecondaryAddress.Length + 3) % 4) % 4;
		byte[] array = new byte[Length];
		WriteCommonFieldsBytes(array);
		int offset = 16;
		LittleEndianWriter.WriteUInt16(array, ref offset, MaxTransmitFragmentSize);
		LittleEndianWriter.WriteUInt16(array, ref offset, MaxReceiveFragmentSize);
		LittleEndianWriter.WriteUInt32(array, ref offset, AssociationGroupID);
		RPCHelper.WritePortAddress(array, ref offset, SecondaryAddress);
		offset += num;
		ResultList.WriteBytes(array, ref offset);
		ByteWriter.WriteBytes(array, offset, AuthVerifier);
		return array;
	}
}
