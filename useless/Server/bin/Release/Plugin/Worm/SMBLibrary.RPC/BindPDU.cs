using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class BindPDU : RPCPDU
{
	public const int BindFieldsFixedLength = 8;

	public ushort MaxTransmitFragmentSize;

	public ushort MaxReceiveFragmentSize;

	public uint AssociationGroupID;

	public ContextList ContextList;

	public byte[] AuthVerifier;

	public override int Length => 24 + ContextList.Length + AuthLength;

	public BindPDU()
	{
		PacketType = PacketTypeName.Bind;
		ContextList = new ContextList();
		AuthVerifier = new byte[0];
	}

	public BindPDU(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		offset += 16;
		MaxTransmitFragmentSize = LittleEndianReader.ReadUInt16(buffer, ref offset);
		MaxReceiveFragmentSize = LittleEndianReader.ReadUInt16(buffer, ref offset);
		AssociationGroupID = LittleEndianReader.ReadUInt32(buffer, ref offset);
		ContextList = new ContextList(buffer, offset);
		offset += ContextList.Length;
		AuthVerifier = ByteReader.ReadBytes(buffer, offset, AuthLength);
	}

	public override byte[] GetBytes()
	{
		AuthLength = (ushort)AuthVerifier.Length;
		byte[] array = new byte[Length];
		WriteCommonFieldsBytes(array);
		int offset = 16;
		LittleEndianWriter.WriteUInt16(array, ref offset, MaxTransmitFragmentSize);
		LittleEndianWriter.WriteUInt16(array, ref offset, MaxReceiveFragmentSize);
		LittleEndianWriter.WriteUInt32(array, ref offset, AssociationGroupID);
		ContextList.WriteBytes(array, ref offset);
		ByteWriter.WriteBytes(array, offset, AuthVerifier);
		return array;
	}
}
