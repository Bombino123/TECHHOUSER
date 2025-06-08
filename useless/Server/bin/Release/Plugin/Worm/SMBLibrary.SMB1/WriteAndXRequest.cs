using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class WriteAndXRequest : SMBAndXCommand
{
	public const int ParametersFixedLength = 24;

	public ushort FID;

	public ulong Offset;

	public uint Timeout;

	public WriteMode WriteMode;

	public ushort Remaining;

	public byte[] Data;

	public override CommandName CommandName => CommandName.SMB_COM_WRITE_ANDX;

	public WriteAndXRequest()
	{
	}

	public WriteAndXRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		Offset = LittleEndianConverter.ToUInt32(SMBParameters, 6);
		Timeout = LittleEndianConverter.ToUInt32(SMBParameters, 10);
		WriteMode = (WriteMode)LittleEndianConverter.ToUInt16(SMBParameters, 14);
		Remaining = LittleEndianConverter.ToUInt16(SMBParameters, 16);
		ushort num = LittleEndianConverter.ToUInt16(SMBParameters, 18);
		uint num2 = LittleEndianConverter.ToUInt16(SMBParameters, 20);
		ushort offset2 = LittleEndianConverter.ToUInt16(SMBParameters, 22);
		if (SMBParameters.Length == 28)
		{
			uint num3 = LittleEndianConverter.ToUInt32(SMBParameters, 24);
			Offset |= (ulong)num3 << 32;
		}
		num2 |= (uint)(num << 16);
		Data = ByteReader.ReadBytes(buffer, offset2, (int)num2);
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
		int num3 = 24;
		if (Offset > uint.MaxValue)
		{
			num3 += 4;
			num2 += 4;
		}
		SMBParameters = new byte[num3];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, FID);
		LittleEndianWriter.WriteUInt32(SMBParameters, 6, (uint)(Offset & 0xFFFFFFFFu));
		LittleEndianWriter.WriteUInt32(SMBParameters, 10, Timeout);
		LittleEndianWriter.WriteUInt16(SMBParameters, 14, (ushort)WriteMode);
		LittleEndianWriter.WriteUInt16(SMBParameters, 16, Remaining);
		LittleEndianWriter.WriteUInt16(SMBParameters, 18, value);
		LittleEndianWriter.WriteUInt16(SMBParameters, 20, (ushort)(num & 0xFFFFu));
		LittleEndianWriter.WriteUInt16(SMBParameters, 22, num2);
		if (Offset > uint.MaxValue)
		{
			uint value2 = (uint)(Offset >> 32);
			LittleEndianWriter.WriteUInt32(SMBParameters, 24, value2);
		}
		int num4 = Data.Length;
		if (isUnicode)
		{
			num4++;
		}
		SMBData = new byte[num4];
		int offset = 0;
		if (isUnicode)
		{
			offset++;
		}
		ByteWriter.WriteBytes(SMBData, ref offset, Data);
		return base.GetBytes(isUnicode);
	}
}
