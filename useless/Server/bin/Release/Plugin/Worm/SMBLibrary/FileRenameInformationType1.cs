using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileRenameInformationType1 : FileInformation
{
	public const int FixedLength = 12;

	public bool ReplaceIfExists;

	public uint RootDirectory;

	private uint FileNameLength;

	public string FileName = string.Empty;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileRenameInformation;

	public override int Length => 12 + FileName.Length * 2;

	public FileRenameInformationType1()
	{
	}

	public FileRenameInformationType1(byte[] buffer, int offset)
	{
		ReplaceIfExists = Conversion.ToBoolean(ByteReader.ReadByte(buffer, offset));
		RootDirectory = LittleEndianConverter.ToUInt32(buffer, offset + 4);
		FileNameLength = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		FileName = ByteReader.ReadUTF16String(buffer, offset + 12, (int)FileNameLength / 2);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		FileNameLength = (uint)(FileName.Length * 2);
		ByteWriter.WriteByte(buffer, offset, Convert.ToByte(ReplaceIfExists));
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, RootDirectory);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, FileNameLength);
		ByteWriter.WriteUTF16String(buffer, offset + 12, FileName);
	}
}
