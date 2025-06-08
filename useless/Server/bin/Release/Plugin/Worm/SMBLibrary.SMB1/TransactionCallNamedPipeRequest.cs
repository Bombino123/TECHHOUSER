using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionCallNamedPipeRequest : TransactionSubcommand
{
	public ushort Priority;

	public byte[] WriteData;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_CALL_NMPIPE;

	public TransactionCallNamedPipeRequest()
	{
	}

	public TransactionCallNamedPipeRequest(byte[] setup, byte[] data)
	{
		Priority = LittleEndianConverter.ToUInt16(setup, 2);
		WriteData = data;
	}

	public override byte[] GetSetup()
	{
		byte[] array = new byte[4];
		LittleEndianWriter.WriteUInt16(array, 0, (ushort)SubcommandName);
		LittleEndianWriter.WriteUInt16(array, 2, Priority);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return WriteData;
	}
}
