using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class DeleteDirectoryRequest : SMB1Command
{
	public const int SupportedBufferFormat = 4;

	public byte BufferFormat;

	public string DirectoryName;

	public override CommandName CommandName => CommandName.SMB_COM_DELETE_DIRECTORY;

	public DeleteDirectoryRequest()
	{
		BufferFormat = 4;
	}

	public DeleteDirectoryRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		BufferFormat = ByteReader.ReadByte(SMBData, 0);
		if (BufferFormat != 4)
		{
			throw new InvalidDataException("Unsupported Buffer Format");
		}
		DirectoryName = SMB1Helper.ReadSMBString(SMBData, 1, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		int num = 1;
		num = ((!isUnicode) ? (num + (DirectoryName.Length + 1)) : (num + (DirectoryName.Length * 2 + 2)));
		SMBData = new byte[num];
		ByteWriter.WriteByte(SMBData, 0, BufferFormat);
		SMB1Helper.WriteSMBString(SMBData, 1, isUnicode, DirectoryName);
		return base.GetBytes(isUnicode);
	}
}
