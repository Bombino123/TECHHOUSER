using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionSetNamedPipeStateResponse : TransactionSubcommand
{
	public const int ParametersLength = 0;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_MAILSLOT_WRITE;
}
