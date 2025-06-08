using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionTransactNamedPipeRequest : TransactionSubcommand
{
	public ushort FID;

	public byte[] WriteData;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_TRANSACT_NMPIPE;

	public TransactionTransactNamedPipeRequest()
	{
	}

	public TransactionTransactNamedPipeRequest(byte[] setup, byte[] data)
	{
		FID = LittleEndianConverter.ToUInt16(setup, 2);
		WriteData = data;
	}

	public override byte[] GetSetup()
	{
		byte[] array = new byte[4];
		LittleEndianWriter.WriteUInt16(array, 0, (ushort)SubcommandName);
		LittleEndianWriter.WriteUInt16(array, 2, FID);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return WriteData;
	}
}
