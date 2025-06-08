using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryInformationRequest : SMB1Command
{
	public const byte SupportedBufferFormat = 4;

	public byte BufferFormat;

	public string FileName;

	public override CommandName CommandName => CommandName.SMB_COM_QUERY_INFORMATION;

	public QueryInformationRequest()
	{
		BufferFormat = 4;
		FileName = string.Empty;
	}

	public QueryInformationRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		BufferFormat = ByteReader.ReadByte(SMBData, 0);
		if (BufferFormat != 4)
		{
			throw new InvalidDataException("Unsupported Buffer Format");
		}
		FileName = SMB1Helper.ReadSMBString(SMBData, 1, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		int num = 1;
		num = ((!isUnicode) ? (num + (FileName.Length + 1)) : (num + (FileName.Length * 2 + 2)));
		SMBData = new byte[1 + num];
		ByteWriter.WriteByte(SMBData, 0, BufferFormat);
		SMB1Helper.WriteSMBString(SMBData, 1, isUnicode, FileName);
		return base.GetBytes(isUnicode);
	}
}
