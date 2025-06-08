using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TreeDisconnectRequest : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_TREE_DISCONNECT;

	public TreeDisconnectRequest()
	{
	}

	public TreeDisconnectRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
