using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionSecondaryRequest : SMB1Command
{
	public const int SMBParametersLength = 16;

	public ushort TotalParameterCount;

	public ushort TotalDataCount;

	protected ushort ParameterCount;

	protected ushort ParameterOffset;

	public ushort ParameterDisplacement;

	protected ushort DataCount;

	protected ushort DataOffset;

	public ushort DataDisplacement;

	public byte[] TransParameters;

	public byte[] TransData;

	public override CommandName CommandName => CommandName.SMB_COM_TRANSACTION_SECONDARY;

	public TransactionSecondaryRequest()
	{
	}

	public TransactionSecondaryRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		TotalParameterCount = LittleEndianConverter.ToUInt16(SMBData, 0);
		TotalDataCount = LittleEndianConverter.ToUInt16(SMBData, 2);
		ParameterCount = LittleEndianConverter.ToUInt16(SMBData, 4);
		ParameterOffset = LittleEndianConverter.ToUInt16(SMBData, 6);
		ParameterDisplacement = LittleEndianConverter.ToUInt16(SMBData, 8);
		DataCount = LittleEndianConverter.ToUInt16(SMBData, 10);
		DataOffset = LittleEndianConverter.ToUInt16(SMBData, 12);
		DataDisplacement = LittleEndianConverter.ToUInt16(SMBData, 14);
		TransParameters = ByteReader.ReadBytes(buffer, ParameterOffset, ParameterCount);
		TransData = ByteReader.ReadBytes(buffer, DataOffset, DataCount);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		ParameterCount = (ushort)TransParameters.Length;
		DataCount = (ushort)TransData.Length;
		ParameterOffset = 51;
		int num = (4 - ParameterOffset % 4) % 4;
		ParameterOffset += (ushort)num;
		DataOffset = (ushort)(ParameterOffset + ParameterCount);
		int num2 = (4 - DataOffset % 4) % 4;
		DataOffset += (ushort)num2;
		SMBParameters = new byte[16];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, TotalParameterCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 2, TotalDataCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, ParameterCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, ParameterOffset);
		LittleEndianWriter.WriteUInt16(SMBParameters, 8, ParameterDisplacement);
		LittleEndianWriter.WriteUInt16(SMBParameters, 10, DataCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 12, DataOffset);
		LittleEndianWriter.WriteUInt16(SMBParameters, 14, DataDisplacement);
		SMBData = new byte[ParameterCount + DataCount + num + num2];
		ByteWriter.WriteBytes(SMBData, num, TransParameters);
		ByteWriter.WriteBytes(SMBData, num + ParameterCount + num2, TransData);
		return base.GetBytes(isUnicode);
	}
}
