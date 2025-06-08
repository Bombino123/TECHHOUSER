using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileRenameInformationType2 : FileInformation
{
	public const int FixedLength = 20;

	public bool ReplaceIfExists;

	public ulong RootDirectory;

	private uint FileNameLength;

	public string FileName = string.Empty;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileRenameInformation;

	public override int Length => Math.Max(20 + FileName.Length * 2, 24);

	public FileRenameInformationType2()
	{
	}

	public FileRenameInformationType2(byte[] buffer, int offset)
	{
		ReplaceIfExists = Conversion.ToBoolean(ByteReader.ReadByte(buffer, offset));
		RootDirectory = LittleEndianConverter.ToUInt64(buffer, offset + 8);
		FileNameLength = LittleEndianConverter.ToUInt32(buffer, offset + 16);
		FileName = ByteReader.ReadUTF16String(buffer, offset + 20, (int)FileNameLength / 2);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		FileNameLength = (uint)(FileName.Length * 2);
		ByteWriter.WriteByte(buffer, offset, Convert.ToByte(ReplaceIfExists));
		LittleEndianWriter.WriteUInt64(buffer, offset + 8, RootDirectory);
		LittleEndianWriter.WriteUInt32(buffer, offset + 16, FileNameLength);
		ByteWriter.WriteUTF16String(buffer, offset + 20, FileName);
	}
}
