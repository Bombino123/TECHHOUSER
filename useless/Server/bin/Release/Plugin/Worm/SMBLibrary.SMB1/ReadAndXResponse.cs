using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class ReadAndXResponse : SMBAndXCommand
{
	public const int ParametersLength = 24;

	public ushort Available;

	public ushort DataCompactionMode;

	public ushort Reserved1;

	public byte[] Reserved2;

	public byte[] Data;

	public override CommandName CommandName => CommandName.SMB_COM_READ_ANDX;

	public ReadAndXResponse()
	{
		Reserved2 = new byte[8];
	}

	public ReadAndXResponse(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		Available = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		DataCompactionMode = LittleEndianConverter.ToUInt16(SMBParameters, 6);
		Reserved1 = LittleEndianConverter.ToUInt16(SMBParameters, 8);
		uint num = LittleEndianConverter.ToUInt16(SMBParameters, 10);
		ushort offset2 = LittleEndianConverter.ToUInt16(SMBParameters, 12);
		ushort num2 = LittleEndianConverter.ToUInt16(SMBParameters, 14);
		Reserved2 = ByteReader.ReadBytes(buffer, 16, 8);
		num |= (uint)(num2 << 16);
		Data = ByteReader.ReadBytes(buffer, offset2, (int)num);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		uint num = (uint)Data.Length;
		ushort num2 = 59;
		if (isUnicode)
		{
			num2++;
		}
		ushort value = (ushort)(num >> 16);
		SMBParameters = new byte[24];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, Available);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, DataCompactionMode);
		LittleEndianWriter.WriteUInt16(SMBParameters, 8, Reserved1);
		LittleEndianWriter.WriteUInt16(SMBParameters, 10, (ushort)(num & 0xFFFFu));
		LittleEndianWriter.WriteUInt16(SMBParameters, 12, num2);
		LittleEndianWriter.WriteUInt16(SMBParameters, 14, value);
		ByteWriter.WriteBytes(SMBParameters, 16, Reserved2);
		int num3 = Data.Length;
		if (isUnicode)
		{
			num3++;
		}
		SMBData = new byte[num3];
		int num4 = 0;
		if (isUnicode)
		{
			num4++;
		}
		ByteWriter.WriteBytes(SMBData, num4, Data);
		return base.GetBytes(isUnicode);
	}
}
