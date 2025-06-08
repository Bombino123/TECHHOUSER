using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactIOCTLResponse : NTTransactSubcommand
{
	public const int ParametersLength = 0;

	public const int SetupLength = 2;

	public ushort TransactionDataSize;

	public byte[] Data;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_IOCTL;

	public NTTransactIOCTLResponse()
	{
	}

	public NTTransactIOCTLResponse(byte[] setup, byte[] data)
	{
		TransactionDataSize = LittleEndianConverter.ToUInt16(setup, 0);
		Data = data;
	}

	public override byte[] GetSetup()
	{
		byte[] array = new byte[2];
		LittleEndianWriter.WriteUInt16(array, 0, TransactionDataSize);
		return array;
	}

	public override byte[] GetData()
	{
		return Data;
	}
}
