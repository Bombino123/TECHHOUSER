using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class QueryInformation
{
	public abstract QueryInformationLevel InformationLevel { get; }

	public abstract byte[] GetBytes();

	public static QueryInformation GetQueryInformation(byte[] buffer, QueryInformationLevel informationLevel)
	{
		return informationLevel switch
		{
			QueryInformationLevel.SMB_QUERY_FILE_BASIC_INFO => new QueryFileBasicInfo(buffer, 0), 
			QueryInformationLevel.SMB_QUERY_FILE_STANDARD_INFO => new QueryFileStandardInfo(buffer, 0), 
			QueryInformationLevel.SMB_QUERY_FILE_EA_INFO => new QueryFileEaInfo(buffer, 0), 
			QueryInformationLevel.SMB_QUERY_FILE_NAME_INFO => new QueryFileNameInfo(buffer, 0), 
			QueryInformationLevel.SMB_QUERY_FILE_ALL_INFO => new QueryFileAllInfo(buffer, 0), 
			QueryInformationLevel.SMB_QUERY_FILE_ALT_NAME_INFO => new QueryFileAltNameInfo(buffer, 0), 
			QueryInformationLevel.SMB_QUERY_FILE_STREAM_INFO => new QueryFileStreamInfo(buffer, 0), 
			QueryInformationLevel.SMB_QUERY_FILE_COMPRESSION_INFO => new QueryFileCompressionInfo(buffer, 0), 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}
}
