using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFileEaInfo : QueryInformation
{
	public uint EaSize;

	public override QueryInformationLevel InformationLevel => QueryInformationLevel.SMB_QUERY_FILE_EA_INFO;

	public QueryFileEaInfo()
	{
	}

	public QueryFileEaInfo(byte[] buffer, int offset)
	{
		EaSize = LittleEndianConverter.ToUInt32(buffer, offset);
	}

	public override byte[] GetBytes()
	{
		return LittleEndianConverter.GetBytes(EaSize);
	}
}
