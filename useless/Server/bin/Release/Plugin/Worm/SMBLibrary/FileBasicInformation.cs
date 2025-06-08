using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileBasicInformation : FileInformation
{
	public const int FixedLength = 40;

	public SetFileTime CreationTime;

	public SetFileTime LastAccessTime;

	public SetFileTime LastWriteTime;

	public SetFileTime ChangeTime;

	public FileAttributes FileAttributes;

	public uint Reserved;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileBasicInformation;

	public override int Length => 40;

	public FileBasicInformation()
	{
	}

	public FileBasicInformation(byte[] buffer, int offset)
	{
		CreationTime = FileTimeHelper.ReadSetFileTime(buffer, offset);
		LastAccessTime = FileTimeHelper.ReadSetFileTime(buffer, offset + 8);
		LastWriteTime = FileTimeHelper.ReadSetFileTime(buffer, offset + 16);
		ChangeTime = FileTimeHelper.ReadSetFileTime(buffer, offset + 24);
		FileAttributes = (FileAttributes)LittleEndianConverter.ToUInt32(buffer, offset + 32);
		Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 36);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		FileTimeHelper.WriteSetFileTime(buffer, offset, CreationTime);
		FileTimeHelper.WriteSetFileTime(buffer, offset + 8, LastAccessTime);
		FileTimeHelper.WriteSetFileTime(buffer, offset + 16, LastWriteTime);
		FileTimeHelper.WriteSetFileTime(buffer, offset + 24, ChangeTime);
		LittleEndianWriter.WriteUInt32(buffer, offset + 32, (uint)FileAttributes);
		LittleEndianWriter.WriteUInt32(buffer, offset + 36, Reserved);
	}
}
