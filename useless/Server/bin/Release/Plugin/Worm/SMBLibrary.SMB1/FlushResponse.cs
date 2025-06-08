using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FlushResponse : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_FLUSH;

	public FlushResponse()
	{
	}

	public FlushResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
