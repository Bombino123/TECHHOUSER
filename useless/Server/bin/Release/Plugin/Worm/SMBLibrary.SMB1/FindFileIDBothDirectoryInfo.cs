using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FindFileIDBothDirectoryInfo : FindInformation
{
	public const int FixedLength = 104;

	public uint FileIndex;

	public DateTime? CreationTime;

	public DateTime? LastAccessTime;

	public DateTime? LastWriteTime;

	public DateTime? LastChangeTime;

	public long EndOfFile;

	public long AllocationSize;

	public ExtendedFileAttributes ExtFileAttributes;

	public uint EASize;

	public byte Reserved;

	public string ShortName;

	public ushort Reserved2;

	public ulong FileID;

	public string FileName;

	public override FindInformationLevel InformationLevel => FindInformationLevel.SMB_FIND_FILE_ID_BOTH_DIRECTORY_INFO;

	public FindFileIDBothDirectoryInfo()
	{
	}

	public FindFileIDBothDirectoryInfo(byte[] buffer, int offset, bool isUnicode)
	{
		NextEntryOffset = LittleEndianReader.ReadUInt32(buffer, ref offset);
		FileIndex = LittleEndianReader.ReadUInt32(buffer, ref offset);
		CreationTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastAccessTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastWriteTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastChangeTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		EndOfFile = LittleEndianReader.ReadInt64(buffer, ref offset);
		AllocationSize = LittleEndianReader.ReadInt64(buffer, ref offset);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianReader.ReadUInt32(buffer, ref offset);
		uint byteCount = LittleEndianReader.ReadUInt32(buffer, ref offset);
		EASize = LittleEndianReader.ReadUInt32(buffer, ref offset);
		byte length = ByteReader.ReadByte(buffer, ref offset);
		Reserved = ByteReader.ReadByte(buffer, ref offset);
		ShortName = ByteReader.ReadUTF16String(buffer, ref offset, 12);
		ShortName = ShortName.Substring(0, length);
		Reserved2 = LittleEndianReader.ReadUInt16(buffer, ref offset);
		FileID = LittleEndianReader.ReadUInt64(buffer, ref offset);
		FileName = SMB1Helper.ReadFixedLengthString(buffer, ref offset, isUnicode, (int)byteCount);
	}

	public override void WriteBytes(byte[] buffer, ref int offset, bool isUnicode)
	{
		uint value = (uint)(isUnicode ? (FileName.Length * 2) : FileName.Length);
		byte value2 = (byte)(ShortName.Length * 2);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, NextEntryOffset);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, FileIndex);
		FileTimeHelper.WriteFileTime(buffer, ref offset, CreationTime);
		FileTimeHelper.WriteFileTime(buffer, ref offset, LastAccessTime);
		FileTimeHelper.WriteFileTime(buffer, ref offset, LastWriteTime);
		FileTimeHelper.WriteFileTime(buffer, ref offset, LastChangeTime);
		LittleEndianWriter.WriteInt64(buffer, ref offset, EndOfFile);
		LittleEndianWriter.WriteInt64(buffer, ref offset, AllocationSize);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, (uint)ExtFileAttributes);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, value);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, EASize);
		ByteWriter.WriteByte(buffer, ref offset, value2);
		ByteWriter.WriteByte(buffer, ref offset, Reserved);
		ByteWriter.WriteUTF16String(buffer, ref offset, ShortName, 12);
		LittleEndianWriter.WriteUInt16(buffer, ref offset, Reserved2);
		LittleEndianWriter.WriteUInt64(buffer, ref offset, FileID);
		SMB1Helper.WriteSMBString(buffer, ref offset, isUnicode, FileName);
	}

	public override int GetLength(bool isUnicode)
	{
		int num = 104;
		if (isUnicode)
		{
			return num + (FileName.Length * 2 + 2);
		}
		return num + (FileName.Length + 1);
	}
}
