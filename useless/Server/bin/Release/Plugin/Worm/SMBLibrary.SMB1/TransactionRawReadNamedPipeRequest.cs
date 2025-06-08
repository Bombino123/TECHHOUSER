using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionRawReadNamedPipeRequest : TransactionSubcommand
{
	public ushort FID;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_RAW_READ_NMPIPE;

	public TransactionRawReadNamedPipeRequest()
	{
	}

	public TransactionRawReadNamedPipeRequest(byte[] setup)
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
