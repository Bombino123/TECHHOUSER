using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileStandardInformation : FileInformation
{
	public const int FixedLength = 24;

	public long AllocationSize;

	public long EndOfFile;

	public uint NumberOfLinks;

	public bool DeletePending;

	public bool Directory;

	public ushort Reserved;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileStandardInformation;

	public override int Length => 24;

	public FileStandardInformation()
	{
	}

	public FileStandardInformation(byte[] buffer, int offset)
	{
		AllocationSize = LittleEndianConverter.ToInt64(buffer, offset);
		EndOfFile = LittleEndianConverter.ToInt64(buffer, offset + 8);
		NumberOfLinks = LittleEndianConverter.ToUInt32(buffer, offset + 16);
		DeletePending = ByteReader.ReadByte(buffer, offset + 20) > 0;
		Directory = ByteReader.ReadByte(buffer, offset + 21) > 0;
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 22);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, AllocationSize);
		LittleEndianWriter.WriteInt64(buffer, offset + 8, EndOfFile);
		LittleEndianWriter.WriteUInt32(buffer, offset + 16, NumberOfLinks);
		ByteWriter.WriteByte(buffer, offset + 20, Convert.ToByte(DeletePending));
		ByteWriter.WriteByte(buffer, offset + 21, Convert.ToByte(Directory));
		LittleEndianWriter.WriteUInt16(buffer, offset + 22, Reserved);
	}
}
