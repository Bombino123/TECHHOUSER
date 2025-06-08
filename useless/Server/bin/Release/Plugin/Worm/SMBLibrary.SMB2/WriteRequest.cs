using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class WriteRequest : SMB2Command
{
	public const int FixedSize = 48;

	public const int DeclaredSize = 49;

	private ushort StructureSize;

	private ushort DataOffset;

	private uint DataLength;

	public ulong Offset;

	public FileID FileId;

	public uint Channel;

	public uint RemainingBytes;

	private ushort WriteChannelInfoOffset;

	private ushort WriteChannelInfoLength;

	public WriteFlags Flags;

	public byte[] Data = new byte[0];

	public byte[] WriteChannelInfo = new byte[0];

	public override int CommandLength => 48 + Data.Length + WriteChannelInfo.Length;

	public WriteRequest()
		: base(SMB2CommandName.Write)
	{
		StructureSize = 49;
	}

	public WriteRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		DataOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		DataLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		Offset = LittleEndianConverter.ToUInt64(buffer, offset + 64 + 8);
		FileId = new FileID(buffer, offset + 64 + 16);
		Channel = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 32);
		RemainingBytes = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 36);
		WriteChannelInfoOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 40);
		WriteChannelInfoLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 42);
		Flags = (WriteFlags)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 44);
		Data = ByteReader.ReadBytes(buffer, offset + DataOffset, (int)DataLength);
		WriteChannelInfo = ByteReader.ReadBytes(buffer, offset + WriteChannelInfoOffset, WriteChannelInfoLength);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		WriteChannelInfoOffset = 0;
		WriteChannelInfoLength = (ushort)WriteChannelInfo.Length;
		if (WriteChannelInfo.Length != 0)
		{
			WriteChannelInfoOffset = 112;
		}
		DataOffset = 0;
		DataLength = (uint)Data.Length;
		if (Data.Length != 0)
		{
			DataOffset = (ushort)(112 + WriteChannelInfo.Length);
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, DataOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, DataLength);
		LittleEndianWriter.WriteUInt64(buffer, offset + 8, Offset);
		FileId.WriteBytes(buffer, offset + 16);
		LittleEndianWriter.WriteUInt32(buffer, offset + 32, Channel);
		LittleEndianWriter.WriteUInt32(buffer, offset + 36, RemainingBytes);
		LittleEndianWriter.WriteUInt16(buffer, offset + 40, WriteChannelInfoOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 42, WriteChannelInfoLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 44, (uint)Flags);
		if (WriteChannelInfo.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 48, WriteChannelInfo);
		}
		if (Data.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 48 + WriteChannelInfo.Length, Data);
		}
	}
}
