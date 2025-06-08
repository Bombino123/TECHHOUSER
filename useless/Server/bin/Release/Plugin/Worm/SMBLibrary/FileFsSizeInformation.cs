using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFsSizeInformation : FileSystemInformation
{
	public const int FixedLength = 24;

	public long TotalAllocationUnits;

	public long AvailableAllocationUnits;

	public uint SectorsPerAllocationUnit;

	public uint BytesPerSector;

	public override FileSystemInformationClass FileSystemInformationClass => FileSystemInformationClass.FileFsSizeInformation;

	public override int Length => 24;

	public FileFsSizeInformation()
	{
	}

	public FileFsSizeInformation(byte[] buffer, int offset)
	{
		TotalAllocationUnits = LittleEndianConverter.ToInt64(buffer, offset);
		AvailableAllocationUnits = LittleEndianConverter.ToInt64(buffer, offset + 8);
		SectorsPerAllocationUnit = LittleEndianConverter.ToUInt32(buffer, offset + 16);
		BytesPerSector = LittleEndianConverter.ToUInt32(buffer, offset + 20);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, TotalAllocationUnits);
		LittleEndianWriter.WriteInt64(buffer, offset + 8, AvailableAllocationUnits);
		LittleEndianWriter.WriteUInt32(buffer, offset + 16, SectorsPerAllocationUnit);
		LittleEndianWriter.WriteUInt32(buffer, offset + 20, BytesPerSector);
	}
}
