using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TreeDisconnectResponse : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_TREE_DISCONNECT;

	public TreeDisconnectResponse()
	{
	}

	public TreeDisconnectResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
