using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFileAllInfo : QueryInformation
{
	public const int FixedLength = 72;

	public DateTime? CreationTime;

	public DateTime? LastAccessTime;

	public DateTime? LastWriteTime;

	public DateTime? LastChangeTime;

	public ExtendedFileAttributes ExtFileAttributes;

	public uint Reserved1;

	public long AllocationSize;

	public long EndOfFile;

	public uint NumberOfLinks;

	public bool DeletePending;

	public bool Directory;

	public ushort Reserved2;

	public uint EaSize;

	public string FileName;

	public override QueryInformationLevel InformationLevel => QueryInformationLevel.SMB_QUERY_FILE_ALL_INFO;

	public QueryFileAllInfo()
	{
	}

	public QueryFileAllInfo(byte[] buffer, int offset)
	{
		CreationTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastAccessTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastWriteTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		LastChangeTime = FileTimeHelper.ReadNullableFileTime(buffer, ref offset);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianReader.ReadUInt32(buffer, ref offset);
		Reserved1 = LittleEndianReader.ReadUInt32(buffer, ref offset);
		AllocationSize = LittleEndianReader.ReadInt64(buffer, ref offset);
		EndOfFile = LittleEndianReader.ReadInt64(buffer, ref offset);
		NumberOfLinks = LittleEndianReader.ReadUInt32(buffer, ref offset);
		DeletePending = ByteReader.ReadByte(buffer, ref offset) > 0;
		Directory = ByteReader.ReadByte(buffer, ref offset) > 0;
		Reserved2 = LittleEndianReader.ReadUInt16(buffer, ref offset);
		EaSize = LittleEndianReader.ReadUInt32(buffer, ref offset);
		uint num = LittleEndianReader.ReadUInt32(buffer, ref offset);
		FileName = ByteReader.ReadUTF16String(buffer, ref offset, (int)(num / 2));
	}

	public override byte[] GetBytes()
	{
		uint num = (uint)(FileName.Length * 2);
		byte[] array = new byte[72 + num];
		int offset = 0;
		FileTimeHelper.WriteFileTime(array, ref offset, CreationTime);
		FileTimeHelper.WriteFileTime(array, ref offset, LastAccessTime);
		FileTimeHelper.WriteFileTime(array, ref offset, LastWriteTime);
		FileTimeHelper.WriteFileTime(array, ref offset, LastChangeTime);
		LittleEndianWriter.WriteUInt32(array, ref offset, (uint)ExtFileAttributes);
		LittleEndianWriter.WriteUInt32(array, ref offset, Reserved1);
		LittleEndianWriter.WriteInt64(array, ref offset, AllocationSize);
		LittleEndianWriter.WriteInt64(array, ref offset, EndOfFile);
		LittleEndianWriter.WriteUInt32(array, ref offset, NumberOfLinks);
		ByteWriter.WriteByte(array, ref offset, Convert.ToByte(DeletePending));
		ByteWriter.WriteByte(array, ref offset, Convert.ToByte(Directory));
		LittleEndianWriter.WriteUInt16(array, ref offset, Reserved2);
		LittleEndianWriter.WriteUInt32(array, ref offset, EaSize);
		LittleEndianWriter.WriteUInt32(array, ref offset, num);
		ByteWriter.WriteUTF16String(array, ref offset, FileName);
		return array;
	}
}
