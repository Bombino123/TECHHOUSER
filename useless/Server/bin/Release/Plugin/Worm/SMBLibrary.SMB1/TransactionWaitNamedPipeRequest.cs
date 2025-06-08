using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionWaitNamedPipeRequest : TransactionSubcommand
{
	public ushort Priority;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_WAIT_NMPIPE;

	public TransactionWaitNamedPipeRequest()
	{
	}

	public TransactionWaitNamedPipeRequest(byte[] setup)
	{
		Priority = LittleEndianConverter.ToUInt16(setup, 2);
	}

	public override byte[] GetSetup()
	{
		byte[] array = new byte[4];
		LittleEndianWriter.WriteUInt16(array, 0, (ushort)SubcommandName);
		LittleEndianWriter.WriteUInt16(array, 2, Priority);
		return array;
	}
}
