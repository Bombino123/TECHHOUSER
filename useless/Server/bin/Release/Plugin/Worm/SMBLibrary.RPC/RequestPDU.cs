using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class RequestPDU : RPCPDU
{
	public const int RequestFieldsFixedLength = 8;

	public uint AllocationHint;

	public ushort ContextID;

	public ushort OpNum;

	public Guid ObjectGuid;

	public byte[] Data;

	public byte[] AuthVerifier;

	public override int Length
	{
		get
		{
			int num = 24 + Data.Length + AuthVerifier.Length;
			if ((int)(Flags & PacketFlags.ObjectUUID) > 0)
			{
				num += 16;
			}
			return num;
		}
	}

	public RequestPDU()
	{
		PacketType = PacketTypeName.Request;
		AuthVerifier = new byte[0];
	}

	public RequestPDU(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		offset += 16;
		AllocationHint = LittleEndianReader.ReadUInt32(buffer, ref offset);
		ContextID = LittleEndianReader.ReadUInt16(buffer, ref offset);
		OpNum = LittleEndianReader.ReadUInt16(buffer, ref offset);
		if ((int)(Flags & PacketFlags.ObjectUUID) > 0)
		{
			ObjectGuid = LittleEndianReader.ReadGuid(buffer, ref offset);
		}
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
		LittleEndianWriter.WriteUInt16(array, ref offset, OpNum);
		if ((int)(Flags & PacketFlags.ObjectUUID) > 0)
		{
			LittleEndianWriter.WriteGuid(array, ref offset, ObjectGuid);
		}
		ByteWriter.WriteBytes(array, ref offset, Data);
		ByteWriter.WriteBytes(array, ref offset, AuthVerifier);
		return array;
	}
}
