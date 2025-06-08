using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TransactionInterimResponse : SMB1Command
{
	public const int ParametersLength = 0;

	public override CommandName CommandName => CommandName.SMB_COM_TRANSACTION;

	public TransactionInterimResponse()
	{
	}

	public TransactionInterimResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		return base.GetBytes(isUnicode);
	}
}
