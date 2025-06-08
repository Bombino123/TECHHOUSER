using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFsSectorSizeInformation : FileSystemInformation
{
	public const int FixedLength = 28;

	public uint LogicalBytesPerSector;

	public uint PhysicalBytesPerSectorForAtomicity;

	public uint PhysicalBytesPerSectorForPerformance;

	public uint FileSystemEffectivePhysicalBytesPerSectorForAtomicity;

	public SectorSizeInformationFlags Flags;

	public uint ByteOffsetForSectorAlignment;

	public uint ByteOffsetForPartitionAlignment;

	public override FileSystemInformationClass FileSystemInformationClass => FileSystemInformationClass.FileFsSectorSizeInformation;

	public override int Length => 28;

	public FileFsSectorSizeInformation()
	{
	}

	public FileFsSectorSizeInformation(byte[] buffer, int offset)
	{
		LogicalBytesPerSector = LittleEndianConverter.ToUInt32(buffer, offset);
		PhysicalBytesPerSectorForAtomicity = LittleEndianConverter.ToUInt32(buffer, offset + 4);
		PhysicalBytesPerSectorForPerformance = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		FileSystemEffectivePhysicalBytesPerSectorForAtomicity = LittleEndianConverter.ToUInt32(buffer, offset + 12);
		Flags = (SectorSizeInformationFlags)LittleEndianConverter.ToUInt32(buffer, offset + 16);
		ByteOffsetForSectorAlignment = LittleEndianConverter.ToUInt32(buffer, offset + 20);
		ByteOffsetForPartitionAlignment = LittleEndianConverter.ToUInt32(buffer, offset + 24);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, offset, LogicalBytesPerSector);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, PhysicalBytesPerSectorForAtomicity);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, PhysicalBytesPerSectorForPerformance);
		LittleEndianWriter.WriteUInt32(buffer, offset + 12, FileSystemEffectivePhysicalBytesPerSectorForAtomicity);
		LittleEndianWriter.WriteUInt32(buffer, offset + 16, (uint)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 20, ByteOffsetForSectorAlignment);
		LittleEndianWriter.WriteUInt32(buffer, offset + 24, ByteOffsetForPartitionAlignment);
	}
}
