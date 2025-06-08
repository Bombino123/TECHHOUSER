using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionCallNamedPipeResponse : TransactionSubcommand
{
	public const int ParametersLength = 0;

	public byte[] ReadData;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_CALL_NMPIPE;

	public TransactionCallNamedPipeResponse()
	{
	}

	public TransactionCallNamedPipeResponse(byte[] data)
	{
		ReadData = data;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return ReadData;
	}
}
