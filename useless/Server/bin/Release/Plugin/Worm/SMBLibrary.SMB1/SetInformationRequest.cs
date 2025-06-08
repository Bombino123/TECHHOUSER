using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SetInformationRequest : SMB1Command
{
	public const int ParametersLength = 16;

	public const int SupportedBufferFormat = 4;

	public SMBFileAttributes FileAttributes;

	public DateTime? LastWriteTime;

	public byte[] Reserved;

	public byte BufferFormat;

	public string FileName;

	public override CommandName CommandName => CommandName.SMB_COM_SET_INFORMATION;

	public SetInformationRequest()
	{
		Reserved = new byte[10];
		BufferFormat = 4;
	}

	public SetInformationRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		FileAttributes = (SMBFileAttributes)LittleEndianConverter.ToUInt16(SMBParameters, 0);
		LastWriteTime = UTimeHelper.ReadNullableUTime(SMBParameters, 2);
		Reserved = ByteReader.ReadBytes(SMBParameters, 6, 10);
		BufferFormat = ByteReader.ReadByte(SMBData, 0);
		if (BufferFormat != 4)
		{
			throw new InvalidDataException("Unsupported Buffer Format");
		}
		FileName = SMB1Helper.ReadSMBString(SMBData, 1, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[16];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, (ushort)FileAttributes);
		UTimeHelper.WriteUTime(SMBParameters, 2, LastWriteTime);
		ByteWriter.WriteBytes(SMBParameters, 6, Reserved, 10);
		int num = 1;
		num = ((!isUnicode) ? (num + (FileName.Length + 1)) : (num + (FileName.Length * 2 + 2)));
		SMBData = new byte[num];
		ByteWriter.WriteByte(SMBData, 0, BufferFormat);
		SMB1Helper.WriteSMBString(SMBData, 1, isUnicode, FileName);
		return base.GetBytes(isUnicode);
	}
}
