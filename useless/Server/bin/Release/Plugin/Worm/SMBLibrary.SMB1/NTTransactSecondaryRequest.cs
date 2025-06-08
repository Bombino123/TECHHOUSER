using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactSecondaryRequest : SMB1Command
{
	public const int SMBParametersLength = 36;

	public byte[] Reserved1;

	public uint TotalParameterCount;

	public uint TotalDataCount;

	public uint ParameterDisplacement;

	public uint DataDisplacement;

	public byte Reserved2;

	public byte[] TransParameters;

	public byte[] TransData;

	public override CommandName CommandName => CommandName.SMB_COM_NT_TRANSACT_SECONDARY;

	public NTTransactSecondaryRequest()
	{
		Reserved1 = new byte[3];
	}

	public NTTransactSecondaryRequest(byte[] buffer, int offset)
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
		Reserved2 = ByteReader.ReadByte(SMBParameters, ref offset2);
		TransParameters = ByteReader.ReadBytes(buffer, (int)offset3, (int)length);
		TransData = ByteReader.ReadBytes(buffer, (int)offset4, (int)length2);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		uint num = (ushort)TransParameters.Length;
		uint num2 = (ushort)TransData.Length;
		uint num3 = 71u;
		int num4 = (int)(4 - num3 % 4) % 4;
		num3 += (ushort)num4;
		uint num5 = (ushort)(num3 + num);
		int num6 = (int)(4 - num5 % 4) % 4;
		num5 += (ushort)num6;
		SMBParameters = new byte[36];
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
		ByteWriter.WriteByte(SMBParameters, ref offset, Reserved2);
		SMBData = new byte[num + num2 + num4 + num6];
		ByteWriter.WriteBytes(SMBData, num4, TransParameters);
		ByteWriter.WriteBytes(SMBData, (int)(num4 + num + num6), TransData);
		return base.GetBytes(isUnicode);
	}
}
