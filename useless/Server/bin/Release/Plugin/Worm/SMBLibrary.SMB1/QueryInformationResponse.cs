using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryInformationResponse : SMB1Command
{
	public const int ParameterLength = 20;

	public SMBFileAttributes FileAttributes;

	public DateTime? LastWriteTime;

	public uint FileSize;

	public byte[] Reserved;

	public override CommandName CommandName => CommandName.SMB_COM_QUERY_INFORMATION;

	public QueryInformationResponse()
	{
		Reserved = new byte[10];
	}

	public QueryInformationResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FileAttributes = (SMBFileAttributes)LittleEndianConverter.ToUInt16(SMBParameters, 0);
		LastWriteTime = UTimeHelper.ReadNullableUTime(SMBParameters, 2);
		FileSize = LittleEndianConverter.ToUInt32(SMBParameters, 6);
		Reserved = ByteReader.ReadBytes(SMBParameters, 10, 10);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[20];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, (ushort)FileAttributes);
		UTimeHelper.WriteUTime(SMBParameters, 2, LastWriteTime);
		LittleEndianWriter.WriteUInt32(SMBParameters, 6, FileSize);
		ByteWriter.WriteBytes(SMBParameters, 10, Reserved, 10);
		return base.GetBytes(isUnicode);
	}
}
