using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class QueryFSInformation
{
	public abstract int Length { get; }

	public abstract QueryFSInformationLevel InformationLevel { get; }

	public abstract byte[] GetBytes(bool isUnicode);

	public static QueryFSInformation GetQueryFSInformation(byte[] buffer, QueryFSInformationLevel informationLevel, bool isUnicode)
	{
		return informationLevel switch
		{
			QueryFSInformationLevel.SMB_QUERY_FS_VOLUME_INFO => new QueryFSVolumeInfo(buffer, 0), 
			QueryFSInformationLevel.SMB_QUERY_FS_SIZE_INFO => new QueryFSSizeInfo(buffer, 0), 
			QueryFSInformationLevel.SMB_QUERY_FS_DEVICE_INFO => new QueryFSDeviceInfo(buffer, 0), 
			QueryFSInformationLevel.SMB_QUERY_FS_ATTRIBUTE_INFO => new QueryFSAttibuteInfo(buffer, 0), 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}
}
