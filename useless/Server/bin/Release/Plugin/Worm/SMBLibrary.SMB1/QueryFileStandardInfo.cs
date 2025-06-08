using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFileStandardInfo : QueryInformation
{
	public const int Length = 22;

	public long AllocationSize;

	public long EndOfFile;

	public uint NumberOfLinks;

	public bool DeletePending;

	public bool Directory;

	public override QueryInformationLevel InformationLevel => QueryInformationLevel.SMB_QUERY_FILE_STANDARD_INFO;

	public QueryFileStandardInfo()
	{
	}

	public QueryFileStandardInfo(byte[] buffer, int offset)
	{
		AllocationSize = LittleEndianReader.ReadInt64(buffer, ref offset);
		EndOfFile = LittleEndianReader.ReadInt64(buffer, ref offset);
		NumberOfLinks = LittleEndianReader.ReadUInt32(buffer, ref offset);
		DeletePending = ByteReader.ReadByte(buffer, ref offset) > 0;
		Directory = ByteReader.ReadByte(buffer, ref offset) > 0;
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[22];
		int offset = 0;
		LittleEndianWriter.WriteInt64(array, ref offset, AllocationSize);
		LittleEndianWriter.WriteInt64(array, ref offset, EndOfFile);
		LittleEndianWriter.WriteUInt32(array, ref offset, NumberOfLinks);
		ByteWriter.WriteByte(array, ref offset, Convert.ToByte(DeletePending));
		ByteWriter.WriteByte(array, ref offset, Convert.ToByte(Directory));
		return array;
	}
}
