using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FindFileFullDirectoryInfo : FindInformation
{
	public const int FixedLength = 68;

	public uint FileIndex;

	public DateTime? CreationTime;

	public DateTime? LastAccessTime;

	public DateTime? LastWriteTime;

	public DateTime? LastAttrChangeTime;

	public long EndOfFile;

	public long AllocationSize;

	public ExtendedFileAttributes ExtFileAttributes;

	public uint EASize;

	public string FileName;

	public override FindInformationLevel InformationLevel => FindInformationLevel.SMB_FIND_FILE_FULL_DIRECTORY_INFO;

	public FindFileFullDirectoryInfo()
	{
	}

	public FindFileFullDirectoryInfo(byte[] buffer, int offset, bool isUnicode)
	{
		NextEntryOffset = LittleEndianReader.ReadUInt32(buffer, ref offset);
		FileIndex = LittleEndianReader.ReadUInt32(buffer, ref offset);
		CreationTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastAccessTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastWriteTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastAttrChangeTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		EndOfFile = LittleEndianReader.ReadInt64(buffer, ref offset);
		AllocationSize = LittleEndianReader.ReadInt64(buffer, ref offset);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianReader.ReadUInt32(buffer, ref offset);
		uint byteCount = LittleEndianReader.ReadUInt32(buffer, ref offset);
		EASize = LittleEndianReader.ReadUInt32(buffer, ref offset);
		FileName = SMB1Helper.ReadFixedLengthString(buffer, ref offset, isUnicode, (int)byteCount);
	}

	public override void WriteBytes(byte[] buffer, ref int offset, bool isUnicode)
	{
		uint value = (byte)(isUnicode ? (FileName.Length * 2) : FileName.Length);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, NextEntryOffset);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, FileIndex);
		FileTimeHelper.WriteFileTime(buffer, ref offset, CreationTime);
		FileTimeHelper.WriteFileTime(buffer, ref offset, LastAccessTime);
		FileTimeHelper.WriteFileTime(buffer, ref offset, LastWriteTime);
		FileTimeHelper.WriteFileTime(buffer, ref offset, LastAttrChangeTime);
		LittleEndianWriter.WriteInt64(buffer, ref offset, EndOfFile);
		LittleEndianWriter.WriteInt64(buffer, ref offset, AllocationSize);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, (uint)ExtFileAttributes);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, value);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, EASize);
		SMB1Helper.WriteSMBString(buffer, ref offset, isUnicode, FileName);
	}

	public override int GetLength(bool isUnicode)
	{
		int num = 68;
		if (isUnicode)
		{
			return num + (FileName.Length * 2 + 2);
		}
		return num + (FileName.Length + 1);
	}
}
