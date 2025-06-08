using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionPeekNamedPipeResponse : TransactionSubcommand
{
	public const int ParametersLength = 6;

	public ushort ReadDataAvailable;

	public ushort MessageBytesLength;

	public NamedPipeState NamedPipeState;

	public byte[] ReadData;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_PEEK_NMPIPE;

	public TransactionPeekNamedPipeResponse()
	{
	}

	public TransactionPeekNamedPipeResponse(byte[] parameters, byte[] data)
	{
		ReadDataAvailable = LittleEndianConverter.ToUInt16(parameters, 0);
		MessageBytesLength = LittleEndianConverter.ToUInt16(parameters, 2);
		NamedPipeState = (NamedPipeState)LittleEndianConverter.ToUInt16(parameters, 4);
		ReadData = data;
	}

	public override byte[] GetParameters()
	{
		byte[] array = new byte[6];
		LittleEndianWriter.WriteUInt16(array, 0, ReadDataAvailable);
		LittleEndianWriter.WriteUInt16(array, 2, MessageBytesLength);
		LittleEndianWriter.WriteUInt16(array, 4, (ushort)NamedPipeState);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return ReadData;
	}
}
