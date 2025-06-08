using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class CloseResponse : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_CLOSE;

	public CloseResponse()
	{
	}

	public CloseResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
