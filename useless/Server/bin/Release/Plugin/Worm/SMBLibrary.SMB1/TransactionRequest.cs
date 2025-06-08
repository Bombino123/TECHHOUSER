using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionRequest : SMB1Command
{
	public const int FixedSMBParametersLength = 28;

	public ushort TotalParameterCount;

	public ushort TotalDataCount;

	public ushort MaxParameterCount;

	public ushort MaxDataCount;

	public byte MaxSetupCount;

	public byte Reserved1;

	public TransactionFlags Flags;

	public uint Timeout;

	public ushort Reserved2;

	public byte Reserved3;

	public byte[] Setup;

	public string Name;

	public byte[] TransParameters;

	public byte[] TransData;

	public override CommandName CommandName => CommandName.SMB_COM_TRANSACTION;

	public TransactionRequest()
	{
		Name = string.Empty;
	}

	public TransactionRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		TotalParameterCount = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		TotalDataCount = LittleEndianConverter.ToUInt16(SMBParameters, 2);
		MaxParameterCount = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		MaxDataCount = LittleEndianConverter.ToUInt16(SMBParameters, 6);
		MaxSetupCount = ByteReader.ReadByte(SMBParameters, 8);
		Reserved1 = ByteReader.ReadByte(SMBParameters, 9);
		Flags = (TransactionFlags)LittleEndianConverter.ToUInt16(SMBParameters, 10);
		Timeout = LittleEndianConverter.ToUInt32(SMBParameters, 12);
		Reserved2 = LittleEndianConverter.ToUInt16(SMBParameters, 16);
		ushort length = LittleEndianConverter.ToUInt16(SMBParameters, 18);
		ushort offset2 = LittleEndianConverter.ToUInt16(SMBParameters, 20);
		ushort length2 = LittleEndianConverter.ToUInt16(SMBParameters, 22);
		ushort offset3 = LittleEndianConverter.ToUInt16(SMBParameters, 24);
		byte b = ByteReader.ReadByte(SMBParameters, 26);
		Reserved3 = ByteReader.ReadByte(SMBParameters, 27);
		Setup = ByteReader.ReadBytes(SMBParameters, 28, b * 2);
		if (SMBData.Length != 0)
		{
			int offset4 = 0;
			if (this is Transaction2Request)
			{
				Name = string.Empty;
				int num = 1;
				offset4 += num;
			}
			else
			{
				if (isUnicode)
				{
					int num2 = 1;
					offset4 += num2;
				}
				Name = SMB1Helper.ReadSMBString(SMBData, ref offset4, isUnicode);
			}
		}
		TransParameters = ByteReader.ReadBytes(buffer, offset2, length);
		TransData = ByteReader.ReadBytes(buffer, offset3, length2);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		if (Setup.Length % 2 > 0)
		{
			throw new Exception("Setup length must be a multiple of 2");
		}
		byte value = (byte)(Setup.Length / 2);
		ushort num = (ushort)TransParameters.Length;
		ushort num2 = (ushort)TransData.Length;
		int num3;
		int num4;
		if (this is Transaction2Request)
		{
			num3 = 0;
			num4 = 1;
		}
		else if (isUnicode)
		{
			num3 = 1;
			num4 = Name.Length * 2 + 2;
		}
		else
		{
			num3 = 0;
			num4 = Name.Length + 1;
		}
		ushort num5 = (ushort)(35 + (28 + Setup.Length + num3 + num4));
		int num6 = (4 - num5 % 4) % 4;
		num5 += (ushort)num6;
		ushort num7 = (ushort)(num5 + num);
		int num8 = (4 - num7 % 4) % 4;
		num7 += (ushort)num8;
		SMBParameters = new byte[28 + Setup.Length];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, TotalParameterCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 2, TotalDataCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, MaxParameterCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, MaxDataCount);
		ByteWriter.WriteByte(SMBParameters, 8, MaxSetupCount);
		ByteWriter.WriteByte(SMBParameters, 9, Reserved1);
		LittleEndianWriter.WriteUInt16(SMBParameters, 10, (ushort)Flags);
		LittleEndianWriter.WriteUInt32(SMBParameters, 12, Timeout);
		LittleEndianWriter.WriteUInt16(SMBParameters, 16, Reserved2);
		LittleEndianWriter.WriteUInt16(SMBParameters, 18, num);
		LittleEndianWriter.WriteUInt16(SMBParameters, 20, num5);
		LittleEndianWriter.WriteUInt16(SMBParameters, 22, num2);
		LittleEndianWriter.WriteUInt16(SMBParameters, 24, num7);
		ByteWriter.WriteByte(SMBParameters, 26, value);
		ByteWriter.WriteByte(SMBParameters, 27, Reserved3);
		ByteWriter.WriteBytes(SMBParameters, 28, Setup);
		SMBData = new byte[num3 + num4 + num6 + num + num8 + num2];
		int offset = num3;
		if (this is Transaction2Request)
		{
			offset += num4;
		}
		else
		{
			SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, Name);
		}
		ByteWriter.WriteBytes(SMBData, offset + num6, TransParameters);
		ByteWriter.WriteBytes(SMBData, offset + num6 + num + num8, TransData);
		return base.GetBytes(isUnicode);
	}
}
