using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SetFileBasicInfo : SetInformation
{
	public const int Length = 40;

	public SetFileTime CreationTime;

	public SetFileTime LastAccessTime;

	public SetFileTime LastWriteTime;

	public SetFileTime LastChangeTime;

	public ExtendedFileAttributes ExtFileAttributes;

	public uint Reserved;

	public override SetInformationLevel InformationLevel => SetInformationLevel.SMB_SET_FILE_BASIC_INFO;

	public SetFileBasicInfo()
	{
	}

	public SetFileBasicInfo(byte[] buffer)
		: this(buffer, 0)
	{
	}

	public SetFileBasicInfo(byte[] buffer, int offset)
	{
		CreationTime = FileTimeHelper.ReadSetFileTime(buffer, offset);
		LastAccessTime = FileTimeHelper.ReadSetFileTime(buffer, offset + 8);
		LastWriteTime = FileTimeHelper.ReadSetFileTime(buffer, offset + 16);
		LastChangeTime = FileTimeHelper.ReadSetFileTime(buffer, offset + 24);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianConverter.ToUInt32(buffer, offset + 32);
		Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 36);
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[40];
		FileTimeHelper.WriteSetFileTime(array, 0, CreationTime);
		FileTimeHelper.WriteSetFileTime(array, 8, LastAccessTime);
		FileTimeHelper.WriteSetFileTime(array, 16, LastWriteTime);
		FileTimeHelper.WriteSetFileTime(array, 24, LastChangeTime);
		LittleEndianWriter.WriteUInt32(array, 32, (uint)ExtFileAttributes);
		LittleEndianWriter.WriteUInt32(array, 36, Reserved);
		return array;
	}
}
