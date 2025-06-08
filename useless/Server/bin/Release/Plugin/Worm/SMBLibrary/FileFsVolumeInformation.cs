using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFsVolumeInformation : FileSystemInformation
{
	public const int FixedLength = 18;

	public DateTime? VolumeCreationTime;

	public uint VolumeSerialNumber;

	private uint VolumeLabelLength;

	public bool SupportsObjects;

	public byte Reserved;

	public string VolumeLabel = string.Empty;

	public override FileSystemInformationClass FileSystemInformationClass => FileSystemInformationClass.FileFsVolumeInformation;

	public override int Length => 18 + VolumeLabel.Length * 2;

	public FileFsVolumeInformation()
	{
	}

	public FileFsVolumeInformation(byte[] buffer, int offset)
	{
		VolumeCreationTime = FileTimeHelper.ReadNullableFileTime(buffer, offset);
		VolumeSerialNumber = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		VolumeLabelLength = LittleEndianConverter.ToUInt32(buffer, offset + 12);
		SupportsObjects = Convert.ToBoolean(ByteReader.ReadByte(buffer, offset + 16));
		Reserved = ByteReader.ReadByte(buffer, offset + 17);
		if (VolumeLabelLength != 0)
		{
			VolumeLabel = ByteReader.ReadUTF16String(buffer, offset + 18, (int)VolumeLabelLength / 2);
		}
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		VolumeLabelLength = (uint)(VolumeLabel.Length * 2);
		FileTimeHelper.WriteFileTime(buffer, offset, VolumeCreationTime);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, VolumeSerialNumber);
		LittleEndianWriter.WriteUInt32(buffer, offset + 12, VolumeLabelLength);
		ByteWriter.WriteByte(buffer, offset + 16, Convert.ToByte(SupportsObjects));
		ByteWriter.WriteByte(buffer, offset + 17, Reserved);
		ByteWriter.WriteUTF16String(buffer, offset + 18, VolumeLabel);
	}
}
