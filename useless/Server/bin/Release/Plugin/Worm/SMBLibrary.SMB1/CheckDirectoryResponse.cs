using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class CheckDirectoryResponse : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_CHECK_DIRECTORY;

	public CheckDirectoryResponse()
	{
	}

	public CheckDirectoryResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		return base.GetBytes(isUnicode);
	}
}
