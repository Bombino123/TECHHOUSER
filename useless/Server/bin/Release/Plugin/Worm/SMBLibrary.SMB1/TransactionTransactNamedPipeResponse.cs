using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionTransactNamedPipeResponse : TransactionSubcommand
{
	public const int ParametersLength = 0;

	public byte[] ReadData;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_TRANSACT_NMPIPE;

	public TransactionTransactNamedPipeResponse()
	{
	}

	public TransactionTransactNamedPipeResponse(byte[] data)
	{
		ReadData = data;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return ReadData;
	}
}
