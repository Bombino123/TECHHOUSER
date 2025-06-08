using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFSAttibuteInfo : QueryFSInformation
{
	public const int FixedLength = 12;

	public FileSystemAttributes FileSystemAttributes;

	public uint MaxFileNameLengthInBytes;

	public string FileSystemName;

	public override int Length => 12 + FileSystemName.Length * 2;

	public override QueryFSInformationLevel InformationLevel => QueryFSInformationLevel.SMB_QUERY_FS_ATTRIBUTE_INFO;

	public QueryFSAttibuteInfo()
	{
	}

	public QueryFSAttibuteInfo(byte[] buffer, int offset)
	{
		FileSystemAttributes = (FileSystemAttributes)LittleEndianConverter.ToUInt32(buffer, offset);
		MaxFileNameLengthInBytes = LittleEndianConverter.ToUInt32(buffer, offset + 4);
		uint num = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		FileSystemName = ByteReader.ReadUTF16String(buffer, offset + 12, (int)(num / 2));
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		uint value = (uint)(FileSystemName.Length * 2);
		byte[] array = new byte[Length];
		LittleEndianWriter.WriteUInt32(array, 0, (uint)FileSystemAttributes);
		LittleEndianWriter.WriteUInt32(array, 4, MaxFileNameLengthInBytes);
		LittleEndianWriter.WriteUInt32(array, 8, value);
		ByteWriter.WriteUTF16String(array, 12, FileSystemName);
		return array;
	}
}
