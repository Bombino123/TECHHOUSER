using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactResponse : SMB1Command
{
	public const int FixedSMBParametersLength = 36;

	public byte[] Reserved1;

	public uint TotalParameterCount;

	public uint TotalDataCount;

	public uint ParameterDisplacement;

	public uint DataDisplacement;

	public byte[] Setup;

	public byte[] TransParameters;

	public byte[] TransData;

	public override CommandName CommandName => CommandName.SMB_COM_NT_TRANSACT;

	public NTTransactResponse()
	{
		Reserved1 = new byte[3];
	}

	public NTTransactResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		int offset2 = 0;
		Reserved1 = ByteReader.ReadBytes(SMBParameters, ref offset2, 3);
		TotalParameterCount = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		TotalDataCount = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		uint length = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		uint offset3 = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		ParameterDisplacement = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		uint length2 = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		uint offset4 = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		DataDisplacement = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		byte b = ByteReader.ReadByte(SMBParameters, ref offset2);
		Setup = ByteReader.ReadBytes(SMBParameters, ref offset, b * 2);
		TransParameters = ByteReader.ReadBytes(buffer, (int)offset3, (int)length);
		TransData = ByteReader.ReadBytes(buffer, (int)offset4, (int)length2);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		byte value = (byte)(Setup.Length / 2);
		uint num = (ushort)TransParameters.Length;
		uint num2 = (ushort)TransData.Length;
		uint num3 = (ushort)(35 + (36 + Setup.Length));
		int num4 = (int)(4 - num3 % 4) % 4;
		num3 += (ushort)num4;
		uint num5 = (ushort)(num3 + num);
		int num6 = (int)(4 - num5 % 4) % 4;
		num5 += (ushort)num6;
		SMBParameters = new byte[36 + Setup.Length];
		int offset = 0;
		ByteWriter.WriteBytes(SMBParameters, ref offset, Reserved1, 3);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, TotalParameterCount);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, TotalDataCount);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, num);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, num3);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, ParameterDisplacement);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, num2);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, num5);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, DataDisplacement);
		ByteWriter.WriteByte(SMBParameters, ref offset, value);
		ByteWriter.WriteBytes(SMBParameters, ref offset, Setup);
		SMBData = new byte[num + num2 + num4 + num6];
		ByteWriter.WriteBytes(SMBData, num4, TransParameters);
		ByteWriter.WriteBytes(SMBData, (int)(num4 + num + num6), TransData);
		return base.GetBytes(isUnicode);
	}

	public static int CalculateMessageSize(int setupLength, int trans2ParametersLength, int trans2DataLength)
	{
		int num = 35 + (36 + setupLength);
		int num2 = (4 - num % 4) % 4;
		num += num2;
		int num3 = num + trans2ParametersLength;
		int num4 = (4 - num3 % 4) % 4;
		int num5 = 36 + setupLength;
		int num6 = trans2ParametersLength + trans2DataLength + num2 + num4;
		return 32 + num5 + num6 + 3;
	}
}
