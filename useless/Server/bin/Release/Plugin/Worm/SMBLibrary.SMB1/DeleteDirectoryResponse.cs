using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class DeleteDirectoryResponse : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_DELETE_DIRECTORY;

	public DeleteDirectoryResponse()
	{
	}

	public DeleteDirectoryResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
