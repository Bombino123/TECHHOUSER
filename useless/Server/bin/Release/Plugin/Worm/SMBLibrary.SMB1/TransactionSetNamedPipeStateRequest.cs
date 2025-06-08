using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionSetNamedPipeStateRequest : TransactionSubcommand
{
	public ushort FID;

	public PipeState PipeState;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_MAILSLOT_WRITE;

	public TransactionSetNamedPipeStateRequest()
	{
	}

	public TransactionSetNamedPipeStateRequest(byte[] setup, byte[] parameters)
	{
		FID = LittleEndianConverter.ToUInt16(setup, 2);
		PipeState = (PipeState)LittleEndianConverter.ToUInt16(parameters, 0);
	}

	public override byte[] GetSetup()
	{
		byte[] array = new byte[4];
		LittleEndianWriter.WriteUInt16(array, 0, (ushort)SubcommandName);
		LittleEndianWriter.WriteUInt16(array, 2, FID);
		return array;
	}

	public override byte[] GetParameters()
	{
		return LittleEndianConverter.GetBytes((ushort)PipeState);
	}
}
