using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NameServicePacketHeader
{
	public const int Length = 12;

	public ushort TransactionID;

	public NameServiceOperation OpCode;

	public OperationFlags Flags;

	public byte ResultCode;

	public ushort QDCount;

	public ushort ANCount;

	public ushort NSCount;

	public ushort ARCount;

	public NameServicePacketHeader()
	{
	}

	public NameServicePacketHeader(byte[] buffer, ref int offset)
		: this(buffer, offset)
	{
		offset += 12;
	}

	public NameServicePacketHeader(byte[] buffer, int offset)
	{
		TransactionID = BigEndianConverter.ToUInt16(buffer, offset);
		ushort num = BigEndianConverter.ToUInt16(buffer, offset + 2);
		ResultCode = (byte)(num & 0xFu);
		Flags = (OperationFlags)((uint)(num >> 4) & 0x7Fu);
		OpCode = (NameServiceOperation)((uint)(num >> 11) & 0x1Fu);
		QDCount = BigEndianConverter.ToUInt16(buffer, offset + 4);
		ANCount = BigEndianConverter.ToUInt16(buffer, offset + 6);
		NSCount = BigEndianConverter.ToUInt16(buffer, offset + 8);
		ARCount = BigEndianConverter.ToUInt16(buffer, offset + 10);
	}

	public void WriteBytes(Stream stream)
	{
		BigEndianWriter.WriteUInt16(stream, TransactionID);
		ushort num = (ushort)(ResultCode & 0xFu);
		num |= (ushort)((uint)Flags << 4);
		num |= (ushort)((uint)OpCode << 11);
		BigEndianWriter.WriteUInt16(stream, num);
		BigEndianWriter.WriteUInt16(stream, QDCount);
		BigEndianWriter.WriteUInt16(stream, ANCount);
		BigEndianWriter.WriteUInt16(stream, NSCount);
		BigEndianWriter.WriteUInt16(stream, ARCount);
	}
}
