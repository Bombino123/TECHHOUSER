using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFSDeviceInfo : QueryFSInformation
{
	public const int FixedLength = 8;

	public DeviceType DeviceType;

	public DeviceCharacteristics DeviceCharacteristics;

	public override int Length => 8;

	public override QueryFSInformationLevel InformationLevel => QueryFSInformationLevel.SMB_QUERY_FS_DEVICE_INFO;

	public QueryFSDeviceInfo()
	{
	}

	public QueryFSDeviceInfo(byte[] buffer, int offset)
	{
		DeviceType = (DeviceType)LittleEndianConverter.ToUInt32(buffer, offset);
		DeviceCharacteristics = (DeviceCharacteristics)LittleEndianConverter.ToUInt32(buffer, offset + 4);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		byte[] array = new byte[Length];
		LittleEndianWriter.WriteUInt32(array, 0, (uint)DeviceType);
		LittleEndianWriter.WriteUInt32(array, 4, (uint)DeviceCharacteristics);
		return array;
	}
}
