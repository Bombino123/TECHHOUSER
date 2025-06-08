using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2InterimResponse : TransactionInterimResponse
{
	public override CommandName CommandName => CommandName.SMB_COM_TRANSACTION2;

	public Transaction2InterimResponse()
	{
	}

	public Transaction2InterimResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
	}
}
