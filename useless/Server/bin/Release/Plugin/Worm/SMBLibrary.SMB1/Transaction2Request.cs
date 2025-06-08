using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2Request : TransactionRequest
{
	public override CommandName CommandName => CommandName.SMB_COM_TRANSACTION2;

	public Transaction2Request()
	{
	}

	public Transaction2Request(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
	}
}
