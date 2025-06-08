using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFileNameInfo : QueryInformation
{
	public string FileName;

	public override QueryInformationLevel InformationLevel => QueryInformationLevel.SMB_QUERY_FILE_NAME_INFO;

	public QueryFileNameInfo()
	{
	}

	public QueryFileNameInfo(byte[] buffer, int offset)
	{
		uint num = LittleEndianConverter.ToUInt32(buffer, 0);
		FileName = ByteReader.ReadUTF16String(buffer, 4, (int)(num / 2));
	}

	public override byte[] GetBytes()
	{
		uint num = (uint)(FileName.Length * 2);
		byte[] array = new byte[4 + num];
		LittleEndianWriter.WriteUInt32(array, 0, num);
		ByteWriter.WriteUTF16String(array, 4, FileName);
		return array;
	}
}
