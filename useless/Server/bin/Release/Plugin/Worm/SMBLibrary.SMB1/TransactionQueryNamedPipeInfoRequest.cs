using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionQueryNamedPipeInfoRequest : TransactionSubcommand
{
	public ushort FID;

	public ushort Level;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_QUERY_NMPIPE_INFO;

	public TransactionQueryNamedPipeInfoRequest()
	{
	}

	public TransactionQueryNamedPipeInfoRequest(byte[] setup, byte[] parameters)
	{
		FID = LittleEndianConverter.ToUInt16(setup, 2);
		Level = LittleEndianConverter.ToUInt16(parameters, 0);
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
		return LittleEndianConverter.GetBytes(Level);
	}
}
