using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionQueryNamedPipeStateRequest : TransactionSubcommand
{
	public ushort FID;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_QUERY_NMPIPE_STATE;

	public TransactionQueryNamedPipeStateRequest()
	{
	}

	public TransactionQueryNamedPipeStateRequest(byte[] setup, byte[] parameters)
	{
		FID = LittleEndianConverter.ToUInt16(setup, 2);
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}
}
