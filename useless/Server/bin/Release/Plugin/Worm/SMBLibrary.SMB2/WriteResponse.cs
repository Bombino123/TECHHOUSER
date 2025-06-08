using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class WriteResponse : SMB2Command
{
	public const int FixedSize = 16;

	public const int DeclaredSize = 17;

	private ushort StructureSize;

	public ushort Reserved;

	public uint Count;

	public uint Remaining;

	private ushort WriteChannelInfoOffset;

	private ushort WriteChannelInfoLength;

	public byte[] WriteChannelInfo = new byte[0];

	public override int CommandLength => 16 + WriteChannelInfo.Length;

	public WriteResponse()
		: base(SMB2CommandName.Write)
	{
		Header.IsResponse = true;
		StructureSize = 17;
	}

	public WriteResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		Count = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		Remaining = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 8);
		WriteChannelInfoOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 12);
		WriteChannelInfoLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 14);
		WriteChannelInfo = ByteReader.ReadBytes(buffer, offset + WriteChannelInfoOffset, WriteChannelInfoLength);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		WriteChannelInfoOffset = 0;
		WriteChannelInfoLength = (ushort)WriteChannelInfo.Length;
		if (WriteChannelInfo.Length != 0)
		{
			WriteChannelInfoOffset = 80;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, Count);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, Remaining);
		LittleEndianWriter.WriteUInt16(buffer, offset + 12, WriteChannelInfoOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 14, WriteChannelInfoLength);
		if (WriteChannelInfo.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 16, WriteChannelInfo);
		}
	}
}
