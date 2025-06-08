using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public abstract class RPCPDU
{
	public const int CommonFieldsLength = 16;

	public byte VersionMajor;

	public byte VersionMinor;

	protected PacketTypeName PacketType;

	public PacketFlags Flags;

	public DataRepresentationFormat DataRepresentation;

	protected ushort FragmentLength;

	public ushort AuthLength;

	public uint CallID;

	public abstract int Length { get; }

	public RPCPDU()
	{
		VersionMajor = 5;
		VersionMinor = 0;
	}

	public RPCPDU(byte[] buffer, int offset)
	{
		VersionMajor = ByteReader.ReadByte(buffer, offset);
		VersionMinor = ByteReader.ReadByte(buffer, offset + 1);
		PacketType = (PacketTypeName)ByteReader.ReadByte(buffer, offset + 2);
		Flags = (PacketFlags)ByteReader.ReadByte(buffer, offset + 3);
		DataRepresentation = new DataRepresentationFormat(buffer, offset + 4);
		FragmentLength = LittleEndianConverter.ToUInt16(buffer, offset + 8);
		AuthLength = LittleEndianConverter.ToUInt16(buffer, offset + 10);
		CallID = LittleEndianConverter.ToUInt32(buffer, offset + 12);
	}

	public abstract byte[] GetBytes();

	public void WriteCommonFieldsBytes(byte[] buffer)
	{
		ByteWriter.WriteByte(buffer, 0, VersionMajor);
		ByteWriter.WriteByte(buffer, 1, VersionMinor);
		ByteWriter.WriteByte(buffer, 2, (byte)PacketType);
		ByteWriter.WriteByte(buffer, 3, (byte)Flags);
		DataRepresentation.WriteBytes(buffer, 4);
		LittleEndianWriter.WriteUInt16(buffer, 8, (ushort)Length);
		LittleEndianWriter.WriteUInt16(buffer, 10, AuthLength);
		LittleEndianWriter.WriteUInt32(buffer, 12, CallID);
	}

	public static RPCPDU GetPDU(byte[] buffer, int offset)
	{
		return (PacketTypeName)ByteReader.ReadByte(buffer, 2) switch
		{
			PacketTypeName.Request => new RequestPDU(buffer, offset), 
			PacketTypeName.Response => new ResponsePDU(buffer, offset), 
			PacketTypeName.Fault => new FaultPDU(buffer, offset), 
			PacketTypeName.Bind => new BindPDU(buffer, offset), 
			PacketTypeName.BindAck => new BindAckPDU(buffer, offset), 
			PacketTypeName.BindNak => new BindNakPDU(buffer, offset), 
			_ => throw new NotImplementedException(), 
		};
	}

	public static ushort GetPDULength(byte[] buffer, int offset)
	{
		return LittleEndianConverter.ToUInt16(buffer, offset + 8);
	}
}
