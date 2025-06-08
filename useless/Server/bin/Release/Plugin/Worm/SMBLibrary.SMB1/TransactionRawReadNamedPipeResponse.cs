using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionRawReadNamedPipeResponse : TransactionSubcommand
{
	public const int ParametersLength = 0;

	public byte[] BytesRead;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_RAW_READ_NMPIPE;

	public TransactionRawReadNamedPipeResponse()
	{
	}

	public TransactionRawReadNamedPipeResponse(byte[] data)
	{
		BytesRead = data;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return BytesRead;
	}
}
