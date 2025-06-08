using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class RenameRequest : SMB1Command
{
	public const int SupportedBufferFormat = 4;

	public const int ParametersLength = 2;

	public SMBFileAttributes SearchAttributes;

	public byte BufferFormat1;

	public string OldFileName;

	public byte BufferFormat2;

	public string NewFileName;

	public override CommandName CommandName => CommandName.SMB_COM_RENAME;

	public RenameRequest()
	{
		BufferFormat1 = 4;
		BufferFormat2 = 4;
	}

	public RenameRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		SearchAttributes = (SMBFileAttributes)LittleEndianConverter.ToUInt16(SMBParameters, 0);
		int offset2 = 0;
		BufferFormat1 = ByteReader.ReadByte(SMBData, ref offset2);
		if (BufferFormat1 != 4)
		{
			throw new InvalidDataException("Unsupported Buffer Format");
		}
		OldFileName = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		BufferFormat2 = ByteReader.ReadByte(SMBData, ref offset2);
		if (BufferFormat2 != 4)
		{
			throw new InvalidDataException("Unsupported Buffer Format");
		}
		if (isUnicode)
		{
			offset2++;
		}
		NewFileName = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[2];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, (ushort)SearchAttributes);
		if (isUnicode)
		{
			int num = 1;
			SMBData = new byte[2 + OldFileName.Length * 2 + NewFileName.Length * 2 + 4 + num];
		}
		else
		{
			SMBData = new byte[2 + OldFileName.Length + NewFileName.Length + 2];
		}
		int offset = 0;
		ByteWriter.WriteByte(SMBData, ref offset, BufferFormat1);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, OldFileName);
		ByteWriter.WriteByte(SMBData, ref offset, BufferFormat2);
		if (isUnicode)
		{
			offset++;
		}
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NewFileName);
		return base.GetBytes(isUnicode);
	}
}
