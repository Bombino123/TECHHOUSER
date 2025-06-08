using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFileCompressionInfo : QueryInformation
{
	public const int Length = 16;

	public long CompressedFileSize;

	public CompressionFormat CompressionFormat;

	public byte CompressionUnitShift;

	public byte ChunkShift;

	public byte ClusterShift;

	public byte[] Reserved;

	public override QueryInformationLevel InformationLevel => QueryInformationLevel.SMB_QUERY_FILE_COMPRESSION_INFO;

	public QueryFileCompressionInfo()
	{
		Reserved = new byte[3];
	}

	public QueryFileCompressionInfo(byte[] buffer, int offset)
	{
		CompressedFileSize = LittleEndianReader.ReadInt64(buffer, ref offset);
		CompressionFormat = (CompressionFormat)LittleEndianReader.ReadUInt16(buffer, ref offset);
		CompressionUnitShift = ByteReader.ReadByte(buffer, ref offset);
		ChunkShift = ByteReader.ReadByte(buffer, ref offset);
		ClusterShift = ByteReader.ReadByte(buffer, ref offset);
		Reserved = ByteReader.ReadBytes(buffer, ref offset, 3);
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[16];
		int offset = 0;
		LittleEndianWriter.WriteInt64(array, ref offset, CompressedFileSize);
		LittleEndianWriter.WriteUInt16(array, ref offset, (ushort)CompressionFormat);
		ByteWriter.WriteByte(array, ref offset, CompressionUnitShift);
		ByteWriter.WriteByte(array, ref offset, ChunkShift);
		ByteWriter.WriteByte(array, ref offset, ClusterShift);
		ByteWriter.WriteBytes(array, ref offset, Reserved, 3);
		return array;
	}
}
