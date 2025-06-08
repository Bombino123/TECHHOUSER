using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2SecondaryRequest : TransactionSecondaryRequest
{
	public new const int SMBParametersLength = 18;

	public ushort FID;

	public override CommandName CommandName => CommandName.SMB_COM_TRANSACTION2_SECONDARY;

	public Transaction2SecondaryRequest()
	{
	}

	public Transaction2SecondaryRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		TotalParameterCount = LittleEndianConverter.ToUInt16(SMBData, 0);
		TotalDataCount = LittleEndianConverter.ToUInt16(SMBData, 2);
		ParameterCount = LittleEndianConverter.ToUInt16(SMBData, 4);
		ParameterOffset = LittleEndianConverter.ToUInt16(SMBData, 6);
		ParameterDisplacement = LittleEndianConverter.ToUInt16(SMBData, 8);
		DataCount = LittleEndianConverter.ToUInt16(SMBData, 10);
		DataOffset = LittleEndianConverter.ToUInt16(SMBData, 12);
		DataDisplacement = LittleEndianConverter.ToUInt16(SMBData, 14);
		FID = LittleEndianConverter.ToUInt16(SMBData, 16);
		TransParameters = ByteReader.ReadBytes(buffer, ParameterOffset, ParameterCount);
		TransData = ByteReader.ReadBytes(buffer, DataOffset, DataCount);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		ParameterCount = (ushort)TransParameters.Length;
		DataCount = (ushort)TransData.Length;
		ParameterOffset = 50;
		int num = (4 - ParameterOffset % 4) % 4;
		ParameterOffset += (ushort)num;
		DataOffset = (ushort)(ParameterOffset + ParameterCount);
		int num2 = (4 - DataOffset % 4) % 4;
		DataOffset += (ushort)num2;
		SMBParameters = new byte[18];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, TotalParameterCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 2, TotalDataCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, ParameterCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, ParameterOffset);
		LittleEndianWriter.WriteUInt16(SMBParameters, 8, ParameterDisplacement);
		LittleEndianWriter.WriteUInt16(SMBParameters, 10, DataCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 12, DataOffset);
		LittleEndianWriter.WriteUInt16(SMBParameters, 14, DataDisplacement);
		LittleEndianWriter.WriteUInt16(SMBParameters, 16, FID);
		SMBData = new byte[ParameterCount + DataCount + num + num2];
		ByteWriter.WriteBytes(SMBData, num, TransParameters);
		ByteWriter.WriteBytes(SMBData, num + ParameterCount + num2, TransData);
		return base.GetBytes(isUnicode);
	}
}
