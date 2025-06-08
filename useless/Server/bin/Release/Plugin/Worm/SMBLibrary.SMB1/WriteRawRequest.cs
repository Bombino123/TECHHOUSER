using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class WriteRawRequest : SMB1Command
{
	public const int ParametersFixedLength = 24;

	public ushort FID;

	public ushort CountOfBytes;

	public ushort Reserved1;

	public uint Offset;

	public uint Timeout;

	public WriteMode WriteMode;

	public uint Reserved2;

	public uint OffsetHigh;

	public byte[] Data;

	public override CommandName CommandName => CommandName.SMB_COM_WRITE_RAW;

	public WriteRawRequest()
	{
		Data = new byte[0];
	}

	public WriteRawRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		CountOfBytes = LittleEndianConverter.ToUInt16(SMBParameters, 2);
		Reserved1 = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		Offset = LittleEndianConverter.ToUInt32(SMBParameters, 6);
		Timeout = LittleEndianConverter.ToUInt32(SMBParameters, 10);
		WriteMode = (WriteMode)LittleEndianConverter.ToUInt16(SMBParameters, 14);
		Reserved2 = LittleEndianConverter.ToUInt32(SMBParameters, 16);
		ushort length = LittleEndianConverter.ToUInt16(SMBParameters, 20);
		ushort offset2 = LittleEndianConverter.ToUInt16(SMBParameters, 22);
		if (SMBParameters.Length == 28)
		{
			OffsetHigh = LittleEndianConverter.ToUInt32(SMBParameters, 24);
		}
		Data = ByteReader.ReadBytes(buffer, offset2, length);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		throw new NotImplementedException();
	}
}
