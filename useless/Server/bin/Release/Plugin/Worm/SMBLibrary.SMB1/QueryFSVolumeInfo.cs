using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFSVolumeInfo : QueryFSInformation
{
	public const int FixedLength = 18;

	public DateTime? VolumeCreationTime;

	public uint SerialNumber;

	private uint VolumeLabelSize;

	public ushort Reserved;

	public string VolumeLabel;

	public override int Length => 18 + VolumeLabel.Length * 2;

	public override QueryFSInformationLevel InformationLevel => QueryFSInformationLevel.SMB_QUERY_FS_VOLUME_INFO;

	public QueryFSVolumeInfo()
	{
		VolumeLabel = string.Empty;
	}

	public QueryFSVolumeInfo(byte[] buffer, int offset)
	{
		VolumeCreationTime = FileTimeHelper.ReadNullableFileTime(buffer, offset);
		SerialNumber = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		VolumeLabelSize = LittleEndianConverter.ToUInt32(buffer, offset + 12);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 16);
		VolumeLabel = ByteReader.ReadUTF16String(buffer, offset + 18, (int)VolumeLabelSize);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		VolumeLabelSize = (uint)(VolumeLabel.Length * 2);
		byte[] array = new byte[Length];
		FileTimeHelper.WriteFileTime(array, 0, VolumeCreationTime);
		LittleEndianWriter.WriteUInt32(array, 8, SerialNumber);
		LittleEndianWriter.WriteUInt32(array, 12, VolumeLabelSize);
		LittleEndianWriter.WriteUInt16(array, 16, Reserved);
		ByteWriter.WriteUTF16String(array, 18, VolumeLabel);
		return array;
	}
}
