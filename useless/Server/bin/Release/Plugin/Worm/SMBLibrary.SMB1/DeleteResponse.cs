using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class DeleteResponse : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_DELETE;

	public DeleteResponse()
	{
	}

	public DeleteResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		return base.GetBytes(isUnicode);
	}
}
