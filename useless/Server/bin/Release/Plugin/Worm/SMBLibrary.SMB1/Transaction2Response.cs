using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2Response : TransactionResponse
{
	public override CommandName CommandName => CommandName.SMB_COM_TRANSACTION2;

	public Transaction2Response()
	{
	}

	public Transaction2Response(byte[] buffer, int offset)
		: base(buffer, offset)
	{
	}
}
