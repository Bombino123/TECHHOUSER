using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFsControlInformation : FileSystemInformation
{
	public const int FixedLength = 48;

	public long FreeSpaceStartFiltering;

	public long FreeSpaceThreshold;

	public long FreeSpaceStopFiltering;

	public ulong DefaultQuotaThreshold;

	public ulong DefaultQuotaLimit;

	public FileSystemControlFlags FileSystemControlFlags;

	public uint Padding;

	public override FileSystemInformationClass FileSystemInformationClass => FileSystemInformationClass.FileFsControlInformation;

	public override int Length => 48;

	public FileFsControlInformation()
	{
	}

	public FileFsControlInformation(byte[] buffer, int offset)
	{
		FreeSpaceStartFiltering = LittleEndianConverter.ToInt64(buffer, offset);
		FreeSpaceThreshold = LittleEndianConverter.ToInt64(buffer, offset + 8);
		FreeSpaceStopFiltering = LittleEndianConverter.ToInt64(buffer, offset + 16);
		DefaultQuotaThreshold = LittleEndianConverter.ToUInt64(buffer, offset + 24);
		DefaultQuotaLimit = LittleEndianConverter.ToUInt64(buffer, offset + 32);
		FileSystemControlFlags = (FileSystemControlFlags)LittleEndianConverter.ToUInt32(buffer, offset + 40);
		Padding = LittleEndianConverter.ToUInt32(buffer, offset + 44);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, FreeSpaceStartFiltering);
		LittleEndianWriter.WriteInt64(buffer, offset + 8, FreeSpaceThreshold);
		LittleEndianWriter.WriteInt64(buffer, offset + 16, FreeSpaceStopFiltering);
		LittleEndianWriter.WriteUInt64(buffer, offset + 24, DefaultQuotaThreshold);
		LittleEndianWriter.WriteUInt64(buffer, offset + 32, DefaultQuotaLimit);
		LittleEndianWriter.WriteUInt32(buffer, offset + 40, (uint)FileSystemControlFlags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 44, Padding);
	}
}
