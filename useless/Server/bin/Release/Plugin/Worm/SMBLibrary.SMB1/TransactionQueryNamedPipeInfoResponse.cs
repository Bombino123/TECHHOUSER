using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionQueryNamedPipeInfoResponse : TransactionSubcommand
{
	public const int ParametersLength = 0;

	public ushort OutputBufferSize;

	public ushort InputBufferSize;

	public byte MaximumInstances;

	public byte CurrentInstances;

	public byte PipeNameLength;

	public string PipeName;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_QUERY_NMPIPE_INFO;

	public TransactionQueryNamedPipeInfoResponse()
	{
	}

	public TransactionQueryNamedPipeInfoResponse(byte[] data, bool isUnicode)
	{
		OutputBufferSize = LittleEndianConverter.ToUInt16(data, 0);
		InputBufferSize = LittleEndianConverter.ToUInt16(data, 2);
		MaximumInstances = ByteReader.ReadByte(data, 4);
		CurrentInstances = ByteReader.ReadByte(data, 5);
		PipeNameLength = ByteReader.ReadByte(data, 6);
		PipeName = SMB1Helper.ReadSMBString(data, 8, isUnicode);
	}

	public override byte[] GetData(bool isUnicode)
	{
		int num = 8;
		num = ((!isUnicode) ? (num + (PipeName.Length + 1)) : (num + (PipeName.Length * 2 + 2)));
		byte[] array = new byte[num];
		LittleEndianWriter.WriteUInt16(array, 0, OutputBufferSize);
		LittleEndianWriter.WriteUInt16(array, 2, InputBufferSize);
		ByteWriter.WriteByte(array, 4, MaximumInstances);
		ByteWriter.WriteByte(array, 5, CurrentInstances);
		ByteWriter.WriteByte(array, 6, PipeNameLength);
		SMB1Helper.WriteSMBString(array, 8, isUnicode, PipeName);
		return array;
	}
}
