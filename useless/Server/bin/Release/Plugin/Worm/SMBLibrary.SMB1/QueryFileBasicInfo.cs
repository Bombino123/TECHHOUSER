using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFileBasicInfo : QueryInformation
{
	public const int Length = 40;

	public DateTime? CreationTime;

	public DateTime? LastAccessTime;

	public DateTime? LastWriteTime;

	public DateTime? LastChangeTime;

	public ExtendedFileAttributes ExtFileAttributes;

	public uint Reserved;

	public override QueryInformationLevel InformationLevel => QueryInformationLevel.SMB_QUERY_FILE_BASIC_INFO;

	public QueryFileBasicInfo()
	{
	}

	public QueryFileBasicInfo(byte[] buffer, int offset)
	{
		CreationTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastAccessTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastWriteTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastChangeTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianReader.ReadUInt32(buffer, ref offset);
		Reserved = LittleEndianReader.ReadUInt32(buffer, ref offset);
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[40];
		int offset = 0;
		FileTimeHelper.WriteFileTime(array, ref offset, CreationTime);
		FileTimeHelper.WriteFileTime(array, ref offset, LastAccessTime);
		FileTimeHelper.WriteFileTime(array, ref offset, LastWriteTime);
		FileTimeHelper.WriteFileTime(array, ref offset, LastChangeTime);
		LittleEndianWriter.WriteUInt32(array, ref offset, (uint)ExtFileAttributes);
		LittleEndianWriter.WriteUInt32(array, ref offset, Reserved);
		return array;
	}
}
