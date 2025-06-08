using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class ReadRequest : SMB2Command
{
	public const int FixedSize = 48;

	public const int DeclaredSize = 49;

	private ushort StructureSize;

	public byte Padding;

	public ReadFlags Flags;

	public uint ReadLength;

	public ulong Offset;

	public FileID FileId;

	public uint MinimumCount;

	public uint Channel;

	public uint RemainingBytes;

	private ushort ReadChannelInfoOffset;

	private ushort ReadChannelInfoLength;

	public byte[] ReadChannelInfo = new byte[0];

	public override int CommandLength => Math.Max(48 + ReadChannelInfo.Length, 49);

	public ReadRequest()
		: base(SMB2CommandName.Read)
	{
		StructureSize = 49;
	}

	public ReadRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Padding = ByteReader.ReadByte(buffer, offset + 64 + 2);
		Flags = (ReadFlags)ByteReader.ReadByte(buffer, offset + 64 + 3);
		ReadLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		Offset = LittleEndianConverter.ToUInt64(buffer, offset + 64 + 8);
		FileId = new FileID(buffer, offset + 64 + 16);
		MinimumCount = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 32);
		Channel = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 36);
		RemainingBytes = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 40);
		ReadChannelInfoOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 44);
		ReadChannelInfoLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 46);
		if (ReadChannelInfoLength > 0)
		{
			ReadChannelInfo = ByteReader.ReadBytes(buffer, offset + ReadChannelInfoOffset, ReadChannelInfoLength);
		}
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		ReadChannelInfoOffset = 0;
		ReadChannelInfoLength = (ushort)ReadChannelInfo.Length;
		if (ReadChannelInfo.Length != 0)
		{
			ReadChannelInfoOffset = 112;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, Padding);
		ByteWriter.WriteByte(buffer, offset + 3, (byte)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, ReadLength);
		LittleEndianWriter.WriteUInt64(buffer, offset + 8, Offset);
		FileId.WriteBytes(buffer, offset + 16);
		LittleEndianWriter.WriteUInt32(buffer, offset + 32, MinimumCount);
		LittleEndianWriter.WriteUInt32(buffer, offset + 36, Channel);
		LittleEndianWriter.WriteUInt32(buffer, offset + 40, RemainingBytes);
		LittleEndianWriter.WriteUInt16(buffer, offset + 44, ReadChannelInfoOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 46, ReadChannelInfoLength);
		if (ReadChannelInfo.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 48, ReadChannelInfo);
		}
		else
		{
			ByteWriter.WriteBytes(buffer, offset + 48, new byte[1]);
		}
	}
}
