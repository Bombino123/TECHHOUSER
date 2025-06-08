using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class WriteRequest : SMB1Command
{
	public const int ParametersLength = 8;

	public const int SupportedBufferFormat = 1;

	public ushort FID;

	public ushort CountOfBytesToWrite;

	public ushort WriteOffsetInBytes;

	public ushort EstimateOfRemainingBytesToBeWritten;

	public byte BufferFormat;

	public byte[] Data;

	public override CommandName CommandName => CommandName.SMB_COM_WRITE;

	public WriteRequest()
	{
		BufferFormat = 1;
		Data = new byte[0];
	}

	public WriteRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		CountOfBytesToWrite = LittleEndianConverter.ToUInt16(SMBParameters, 2);
		WriteOffsetInBytes = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		EstimateOfRemainingBytesToBeWritten = LittleEndianConverter.ToUInt16(SMBParameters, 6);
		BufferFormat = ByteReader.ReadByte(SMBData, 0);
		if (BufferFormat != 1)
		{
			throw new InvalidDataException("Unsupported Buffer Format");
		}
		ushort length = LittleEndianConverter.ToUInt16(SMBData, 1);
		Data = ByteReader.ReadBytes(SMBData, 3, length);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		if (Data.Length > 65535)
		{
			throw new ArgumentException("Invalid Data length");
		}
		SMBParameters = new byte[8];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, FID);
		LittleEndianWriter.WriteUInt16(SMBParameters, 2, CountOfBytesToWrite);
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, WriteOffsetInBytes);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, EstimateOfRemainingBytesToBeWritten);
		SMBData = new byte[3 + Data.Length];
		ByteWriter.WriteByte(SMBData, 0, BufferFormat);
		LittleEndianWriter.WriteUInt16(SMBData, 1, (ushort)Data.Length);
		ByteWriter.WriteBytes(SMBData, 3, Data);
		return base.GetBytes(isUnicode);
	}
}
