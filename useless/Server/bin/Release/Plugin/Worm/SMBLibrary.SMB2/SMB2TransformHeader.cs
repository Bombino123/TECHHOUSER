using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class SMB2TransformHeader
{
	public const int Length = 52;

	public const int SignatureLength = 16;

	public const int NonceLength = 16;

	private const int NonceStartOffset = 20;

	public static readonly byte[] ProtocolSignature = new byte[4] { 253, 83, 77, 66 };

	private byte[] ProtocolId;

	public byte[] Signature;

	public byte[] Nonce;

	public uint OriginalMessageSize;

	public ushort Reserved;

	public SMB2TransformHeaderFlags Flags;

	public ulong SessionId;

	public SMB2TransformHeader()
	{
		ProtocolId = ProtocolSignature;
	}

	public SMB2TransformHeader(byte[] buffer, int offset)
	{
		ProtocolId = ByteReader.ReadBytes(buffer, offset, 4);
		Signature = ByteReader.ReadBytes(buffer, offset + 4, 16);
		Nonce = ByteReader.ReadBytes(buffer, offset + 20, 16);
		OriginalMessageSize = LittleEndianConverter.ToUInt32(buffer, offset + 36);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 40);
		Flags = (SMB2TransformHeaderFlags)LittleEndianConverter.ToUInt16(buffer, offset + 42);
		SessionId = LittleEndianConverter.ToUInt64(buffer, offset + 44);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		ByteWriter.WriteBytes(buffer, offset, ProtocolId);
		ByteWriter.WriteBytes(buffer, offset + 4, Signature);
		WriteAssociatedData(buffer, offset + 20);
	}

	private void WriteAssociatedData(byte[] buffer, int offset)
	{
		ByteWriter.WriteBytes(buffer, offset, Nonce);
		LittleEndianWriter.WriteUInt32(buffer, offset + 16, OriginalMessageSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 20, Reserved);
		LittleEndianWriter.WriteUInt16(buffer, offset + 22, (ushort)Flags);
		LittleEndianWriter.WriteUInt64(buffer, offset + 24, SessionId);
	}

	public byte[] GetAssociatedData()
	{
		byte[] array = new byte[32];
		WriteAssociatedData(array, 0);
		return array;
	}

	public static bool IsTransformHeader(byte[] buffer, int offset)
	{
		byte[] array = ByteReader.ReadBytes(buffer, offset, 4);
		return ByteUtils.AreByteArraysEqual(ProtocolSignature, array);
	}
}
