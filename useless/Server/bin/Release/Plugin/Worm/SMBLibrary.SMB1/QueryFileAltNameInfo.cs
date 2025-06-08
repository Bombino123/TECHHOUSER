using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFileAltNameInfo : QueryInformation
{
	public string FileName;

	public override QueryInformationLevel InformationLevel => QueryInformationLevel.SMB_QUERY_FILE_ALT_NAME_INFO;

	public QueryFileAltNameInfo()
	{
	}

	public QueryFileAltNameInfo(byte[] buffer, int offset)
	{
		uint num = LittleEndianReader.ReadUInt32(buffer, ref offset);
		FileName = ByteReader.ReadUTF16String(buffer, ref offset, (int)(num / 2));
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
