using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionRawWriteNamedPipeResponse : TransactionSubcommand
{
	public const int ParametersLength = 2;

	public ushort BytesWritten;

	public override TransactionSubcommandName SubcommandName => TransactionSubcommandName.TRANS_RAW_WRITE_NMPIPE;

	public TransactionRawWriteNamedPipeResponse()
	{
	}

	public TransactionRawWriteNamedPipeResponse(byte[] parameters)
	{
		BytesWritten = LittleEndianConverter.ToUInt16(parameters, 0);
	}

	public override byte[] GetParameters()
	{
		return LittleEndianConverter.GetBytes(BytesWritten);
	}
}
