using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTCancelRequest : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_NT_CANCEL;

	public NTCancelRequest()
	{
	}

	public NTCancelRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
