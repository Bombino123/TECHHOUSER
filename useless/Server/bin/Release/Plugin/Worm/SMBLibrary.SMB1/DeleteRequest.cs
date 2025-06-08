using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class DeleteRequest : SMB1Command
{
	public const int SupportedBufferFormat = 4;

	public const int ParametersLength = 2;

	public SMBFileAttributes SearchAttributes;

	public byte BufferFormat;

	public string FileName;

	public override CommandName CommandName => CommandName.SMB_COM_DELETE;

	public DeleteRequest()
	{
		BufferFormat = 4;
	}

	public DeleteRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		SearchAttributes = (SMBFileAttributes)LittleEndianConverter.ToUInt16(SMBParameters, 0);
		BufferFormat = ByteReader.ReadByte(SMBData, 0);
		if (BufferFormat != 4)
		{
			throw new InvalidDataException("Unsupported Buffer Format");
		}
		FileName = SMB1Helper.ReadSMBString(SMBData, 1, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		throw new NotImplementedException();
	}
}
