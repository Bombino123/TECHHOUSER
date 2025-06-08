using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionReadNamedPipeResponse : TransactionSubcommand
{
	public const int ParametersLength = 0;

	public byte[] ReadData;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_READ_NMPIPE;

	public TransactionReadNamedPipeResponse()
	{
	}

	public TransactionReadNamedPipeResponse(byte[] data)
	{
		ReadData = data;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return ReadData;
	}
}
