using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFsDeviceInformation : FileSystemInformation
{
	public const int FixedLength = 8;

	public DeviceType DeviceType;

	public DeviceCharacteristics Characteristics;

	public override FileSystemInformationClass FileSystemInformationClass => FileSystemInformationClass.FileFsDeviceInformation;

	public override int Length => 8;

	public FileFsDeviceInformation()
	{
	}

	public FileFsDeviceInformation(byte[] buffer, int offset)
	{
		DeviceType = (DeviceType)LittleEndianConverter.ToUInt32(buffer, offset);
		Characteristics = (DeviceCharacteristics)LittleEndianConverter.ToUInt32(buffer, offset + 4);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, offset, (uint)DeviceType);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, (uint)Characteristics);
	}
}
