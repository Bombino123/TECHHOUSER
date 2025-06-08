using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactRequest : SMB1Command
{
	public const int FixedSMBParametersLength = 38;

	public byte MaxSetupCount;

	public ushort Reserved1;

	public uint TotalParameterCount;

	public uint TotalDataCount;

	public uint MaxParameterCount;

	public uint MaxDataCount;

	public NTTransactSubcommandName Function;

	public byte[] Setup;

	public byte[] TransParameters;

	public byte[] TransData;

	public override CommandName CommandName => CommandName.SMB_COM_NT_TRANSACT;

	public NTTransactRequest()
	{
	}

	public NTTransactRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		int offset2 = 0;
		MaxSetupCount = ByteReader.ReadByte(SMBParameters, ref offset2);
		Reserved1 = LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		TotalParameterCount = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		TotalDataCount = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		MaxParameterCount = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		MaxDataCount = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		uint length = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		uint offset3 = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		uint length2 = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		uint offset4 = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		byte b = ByteReader.ReadByte(SMBParameters, ref offset2);
		Function = (NTTransactSubcommandName)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		Setup = ByteReader.ReadBytes(SMBParameters, ref offset2, b * 2);
		TransParameters = ByteReader.ReadBytes(buffer, (int)offset3, (int)length);
		TransData = ByteReader.ReadBytes(buffer, (int)offset4, (int)length2);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		byte value = (byte)(Setup.Length / 2);
		uint num = (ushort)TransParameters.Length;
		uint num2 = (ushort)TransData.Length;
		uint num3 = (ushort)(35 + (38 + Setup.Length));
		int num4 = (int)(4 - num3 % 4) % 4;
		num3 += (ushort)num4;
		uint num5 = (ushort)(num3 + num);
		int num6 = (int)(4 - num5 % 4) % 4;
		num5 += (ushort)num6;
		SMBParameters = new byte[38 + Setup.Length];
		int offset = 0;
		ByteWriter.WriteByte(SMBParameters, ref offset, MaxSetupCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, Reserved1);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, TotalParameterCount);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, TotalDataCount);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, MaxParameterCount);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, MaxDataCount);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, num);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, num3);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, num2);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, num5);
		ByteWriter.WriteByte(SMBParameters, ref offset, value);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)Function);
		ByteWriter.WriteBytes(SMBParameters, ref offset, Setup);
		SMBData = new byte[num4 + num + num6 + num2];
		ByteWriter.WriteBytes(SMBData, num4, TransParameters);
		ByteWriter.WriteBytes(SMBData, (int)(num4 + num + num6), TransData);
		return base.GetBytes(isUnicode);
	}
}
