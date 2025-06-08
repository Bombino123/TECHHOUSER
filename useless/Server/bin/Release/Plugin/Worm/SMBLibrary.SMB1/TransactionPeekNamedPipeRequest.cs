using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionPeekNamedPipeRequest : TransactionSubcommand
{
	public ushort FID;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_PEEK_NMPIPE;

	public TransactionPeekNamedPipeRequest()
	{
	}

	public TransactionPeekNamedPipeRequest(byte[] setup)
	{
		FID = LittleEndianConverter.ToUInt16(setup, 2);
	}

	public override byte[] GetSetup()
	{
		byte[] array = new byte[4];
		LittleEndianWriter.WriteUInt16(array, 0, (ushort)SubcommandName);
		LittleEndianWriter.WriteUInt16(array, 2, FID);
		return array;
	}
}
