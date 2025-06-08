using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionResponse : SMB1Command
{
	public const int FixedSMBParametersLength = 20;

	public ushort TotalParameterCount;

	public ushort TotalDataCount;

	public ushort Reserved1;

	public ushort ParameterDisplacement;

	public ushort DataDisplacement;

	public byte Reserved2;

	public byte[] Setup;

	public byte[] TransParameters;

	public byte[] TransData;

	public override CommandName CommandName => CommandName.SMB_COM_TRANSACTION;

	public TransactionResponse()
	{
		Setup = new byte[0];
		TransParameters = new byte[0];
		TransData = new byte[0];
	}

	public TransactionResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		TotalParameterCount = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		TotalDataCount = LittleEndianConverter.ToUInt16(SMBParameters, 2);
		Reserved1 = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		ushort length = LittleEndianConverter.ToUInt16(SMBParameters, 6);
		ushort offset2 = LittleEndianConverter.ToUInt16(SMBParameters, 8);
		ParameterDisplacement = LittleEndianConverter.ToUInt16(SMBParameters, 10);
		ushort length2 = LittleEndianConverter.ToUInt16(SMBParameters, 12);
		ushort offset3 = LittleEndianConverter.ToUInt16(SMBParameters, 14);
		DataDisplacement = LittleEndianConverter.ToUInt16(SMBParameters, 16);
		byte b = ByteReader.ReadByte(SMBParameters, 18);
		Reserved2 = ByteReader.ReadByte(SMBParameters, 19);
		Setup = ByteReader.ReadBytes(SMBParameters, 20, b * 2);
		TransParameters = ByteReader.ReadBytes(buffer, offset2, length);
		TransData = ByteReader.ReadBytes(buffer, offset3, length2);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		if (TransData.Length > 65535)
		{
			throw new ArgumentException("Invalid Trans_Data length");
		}
		byte value = (byte)(Setup.Length / 2);
		ushort num = (ushort)TransParameters.Length;
		ushort num2 = (ushort)TransData.Length;
		ushort num3 = (ushort)(35 + (20 + Setup.Length));
		int num4 = (4 - num3 % 4) % 4;
		num3 += (ushort)num4;
		ushort num5 = (ushort)(num3 + num);
		int num6 = (4 - num5 % 4) % 4;
		num5 += (ushort)num6;
		SMBParameters = new byte[20 + Setup.Length];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, TotalParameterCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 2, TotalDataCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, Reserved1);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, num);
		LittleEndianWriter.WriteUInt16(SMBParameters, 8, num3);
		LittleEndianWriter.WriteUInt16(SMBParameters, 10, ParameterDisplacement);
		LittleEndianWriter.WriteUInt16(SMBParameters, 12, num2);
		LittleEndianWriter.WriteUInt16(SMBParameters, 14, num5);
		LittleEndianWriter.WriteUInt16(SMBParameters, 16, DataDisplacement);
		ByteWriter.WriteByte(SMBParameters, 18, value);
		ByteWriter.WriteByte(SMBParameters, 19, Reserved2);
		ByteWriter.WriteBytes(SMBParameters, 20, Setup);
		SMBData = new byte[num + num2 + num4 + num6];
		ByteWriter.WriteBytes(SMBData, num4, TransParameters);
		ByteWriter.WriteBytes(SMBData, num4 + num + num6, TransData);
		return base.GetBytes(isUnicode);
	}

	public static int CalculateMessageSize(int setupLength, int trans2ParametersLength, int trans2DataLength)
	{
		int num = 35 + (20 + setupLength);
		int num2 = (4 - num % 4) % 4;
		num += num2;
		int num3 = num + trans2ParametersLength;
		int num4 = (4 - num3 % 4) % 4;
		int num5 = 20 + setupLength;
		int num6 = trans2ParametersLength + trans2DataLength + num2 + num4;
		return 32 + num5 + num6 + 3;
	}
}
