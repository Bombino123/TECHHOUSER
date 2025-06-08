using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFsFullSizeInformation : FileSystemInformation
{
	public const int FixedLength = 32;

	public long TotalAllocationUnits;

	public long CallerAvailableAllocationUnits;

	public long ActualAvailableAllocationUnits;

	public uint SectorsPerAllocationUnit;

	public uint BytesPerSector;

	public override FileSystemInformationClass FileSystemInformationClass => FileSystemInformationClass.FileFsFullSizeInformation;

	public override int Length => 32;

	public FileFsFullSizeInformation()
	{
	}

	public FileFsFullSizeInformation(byte[] buffer, int offset)
	{
		TotalAllocationUnits = LittleEndianConverter.ToInt64(buffer, offset);
		CallerAvailableAllocationUnits = LittleEndianConverter.ToInt64(buffer, offset + 8);
		ActualAvailableAllocationUnits = LittleEndianConverter.ToInt64(buffer, offset + 16);
		SectorsPerAllocationUnit = LittleEndianConverter.ToUInt32(buffer, offset + 24);
		BytesPerSector = LittleEndianConverter.ToUInt32(buffer, offset + 28);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, TotalAllocationUnits);
		LittleEndianWriter.WriteInt64(buffer, offset + 8, CallerAvailableAllocationUnits);
		LittleEndianWriter.WriteInt64(buffer, offset + 16, ActualAvailableAllocationUnits);
		LittleEndianWriter.WriteUInt32(buffer, offset + 24, SectorsPerAllocationUnit);
		LittleEndianWriter.WriteUInt32(buffer, offset + 28, BytesPerSector);
	}
}
