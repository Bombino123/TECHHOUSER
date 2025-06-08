using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactInterimResponse : SMB1Command
{
	public const int ParametersLength = 0;

	public override CommandName CommandName => CommandName.SMB_COM_NT_TRANSACT;

	public NTTransactInterimResponse()
	{
	}

	public NTTransactInterimResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
