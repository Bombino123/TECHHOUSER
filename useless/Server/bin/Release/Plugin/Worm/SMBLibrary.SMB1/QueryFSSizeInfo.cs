using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFSSizeInfo : QueryFSInformation
{
	public const int FixedLength = 24;

	public long TotalAllocationUnits;

	public long TotalFreeAllocationUnits;

	public uint SectorsPerAllocationUnit;

	public uint BytesPerSector;

	public override int Length => 24;

	public override QueryFSInformationLevel InformationLevel => QueryFSInformationLevel.SMB_QUERY_FS_SIZE_INFO;

	public QueryFSSizeInfo()
	{
	}

	public QueryFSSizeInfo(byte[] buffer, int offset)
	{
		TotalAllocationUnits = LittleEndianConverter.ToInt64(buffer, 0);
		TotalFreeAllocationUnits = LittleEndianConverter.ToInt64(buffer, 8);
		SectorsPerAllocationUnit = LittleEndianConverter.ToUInt32(buffer, 16);
		BytesPerSector = LittleEndianConverter.ToUInt32(buffer, 20);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		byte[] array = new byte[Length];
		LittleEndianWriter.WriteInt64(array, 0, TotalAllocationUnits);
		LittleEndianWriter.WriteInt64(array, 8, TotalFreeAllocationUnits);
		LittleEndianWriter.WriteUInt32(array, 16, SectorsPerAllocationUnit);
		LittleEndianWriter.WriteUInt32(array, 20, BytesPerSector);
		return array;
	}
}
