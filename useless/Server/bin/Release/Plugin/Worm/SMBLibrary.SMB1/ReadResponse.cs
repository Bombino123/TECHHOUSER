using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class ReadResponse : SMB1Command
{
	public const int ParametersLength = 10;

	public const int SupportedBufferFormat = 1;

	public ushort CountOfBytesReturned;

	public byte[] Reserved;

	public byte BufferFormat;

	public byte[] Bytes;

	public override CommandName CommandName => CommandName.SMB_COM_READ;

	public ReadResponse()
	{
		Reserved = new byte[8];
	}

	public ReadResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		CountOfBytesReturned = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		Reserved = ByteReader.ReadBytes(SMBParameters, 2, 8);
		BufferFormat = ByteReader.ReadByte(SMBData, 0);
		if (BufferFormat != 1)
		{
			throw new InvalidDataException("Unsupported Buffer Format");
		}
		ushort length = LittleEndianConverter.ToUInt16(SMBData, 1);
		Bytes = ByteReader.ReadBytes(SMBData, 3, length);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[10];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, CountOfBytesReturned);
		ByteWriter.WriteBytes(SMBParameters, 2, Reserved, 8);
		SMBData = new byte[3 + Bytes.Length];
		ByteWriter.WriteByte(SMBData, 0, BufferFormat);
		LittleEndianWriter.WriteUInt16(SMBData, 1, (ushort)Bytes.Length);
		ByteWriter.WriteBytes(SMBData, 3, Bytes);
		return base.GetBytes(isUnicode);
	}
}
