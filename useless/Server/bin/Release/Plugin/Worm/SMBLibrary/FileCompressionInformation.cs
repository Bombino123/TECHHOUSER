using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileCompressionInformation : FileInformation
{
	public const int FixedLength = 16;

	public long CompressedFileSize;

	public CompressionFormat CompressionFormat;

	public byte CompressionUnitShift;

	public byte ChunkShift;

	public byte ClusterShift;

	public byte[] Reserved;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileCompressionInformation;

	public override int Length => 16;

	public FileCompressionInformation()
	{
	}

	public FileCompressionInformation(byte[] buffer, int offset)
	{
		CompressedFileSize = LittleEndianConverter.ToInt64(buffer, offset);
		CompressionFormat = (CompressionFormat)LittleEndianConverter.ToUInt16(buffer, offset + 8);
		CompressionUnitShift = ByteReader.ReadByte(buffer, offset + 10);
		ChunkShift = ByteReader.ReadByte(buffer, offset + 11);
		ClusterShift = ByteReader.ReadByte(buffer, offset + 12);
		Reserved = ByteReader.ReadBytes(buffer, offset + 13, 3);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, CompressedFileSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 8, (ushort)CompressionFormat);
		ByteWriter.WriteByte(buffer, offset + 10, CompressionUnitShift);
		ByteWriter.WriteByte(buffer, offset + 11, ChunkShift);
		ByteWriter.WriteByte(buffer, offset + 12, ClusterShift);
		ByteWriter.WriteBytes(buffer, offset + 13, Reserved, 3);
	}
}
