using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class CreateDirectoryResponse : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_CREATE_DIRECTORY;

	public CreateDirectoryResponse()
	{
	}

	public CreateDirectoryResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
