using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class ResponsePDU : RPCPDU
{
	public const int ResponseFieldsLength = 8;

	public uint AllocationHint;

	public ushort ContextID;

	public byte CancelCount;

	public byte Reserved;

	public byte[] Data;

	public byte[] AuthVerifier;

	public override int Length => 24 + Data.Length + AuthVerifier.Length;

	public ResponsePDU()
	{
		PacketType = PacketTypeName.Response;
		AuthVerifier = new byte[0];
	}

	public ResponsePDU(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		offset += 16;
		AllocationHint = LittleEndianReader.ReadUInt32(buffer, ref offset);
		ContextID = LittleEndianReader.ReadUInt16(buffer, ref offset);
		CancelCount = ByteReader.ReadByte(buffer, ref offset);
		Reserved = ByteReader.ReadByte(buffer, ref offset);
		int length = FragmentLength - AuthLength - offset;
		Data = ByteReader.ReadBytes(buffer, ref offset, length);
		AuthVerifier = ByteReader.ReadBytes(buffer, offset, AuthLength);
	}

	public override byte[] GetBytes()
	{
		AuthLength = (ushort)AuthVerifier.Length;
		byte[] array = new byte[Length];
		WriteCommonFieldsBytes(array);
		int offset = 16;
		LittleEndianWriter.WriteUInt32(array, ref offset, AllocationHint);
		LittleEndianWriter.WriteUInt16(array, ref offset, ContextID);
		ByteWriter.WriteByte(array, ref offset, CancelCount);
		ByteWriter.WriteByte(array, ref offset, Reserved);
		ByteWriter.WriteBytes(array, ref offset, Data);
		ByteWriter.WriteBytes(array, ref offset, AuthVerifier);
		return array;
	}
}
