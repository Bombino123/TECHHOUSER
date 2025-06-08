using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class RenameResponse : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_RENAME;

	public RenameResponse()
	{
	}

	public RenameResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
