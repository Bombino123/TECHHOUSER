using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionWriteNamedPipeResponse : TransactionSubcommand
{
	public const int ParametersLength = 2;

	public ushort BytesWritten;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_WRITE_NMPIPE;

	public TransactionWriteNamedPipeResponse()
	{
	}

	public TransactionWriteNamedPipeResponse(byte[] parameters)
	{
		BytesWritten = LittleEndianConverter.ToUInt16(parameters, 0);
	}

	public override byte[] GetParameters()
	{
		return LittleEndianConverter.GetBytes(BytesWritten);
	}
}
