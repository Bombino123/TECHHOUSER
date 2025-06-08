using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionQueryNamedPipeStateResponse : TransactionSubcommand
{
	public const int ParametersLength = 2;

	public NamedPipeStatus NMPipeStatus;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_QUERY_NMPIPE_STATE;

	public TransactionQueryNamedPipeStateResponse()
	{
	}

	public TransactionQueryNamedPipeStateResponse(byte[] parameters)
	{
		NMPipeStatus = new NamedPipeStatus(LittleEndianConverter.ToUInt16(parameters, 0));
	}

	public override byte[] GetSetup()
	{
		return new byte[0];
	}

	public override byte[] GetParameters()
	{
		byte[] array = new byte[2];
		NMPipeStatus.WriteBytes(array, 0);
		return array;
	}
}
