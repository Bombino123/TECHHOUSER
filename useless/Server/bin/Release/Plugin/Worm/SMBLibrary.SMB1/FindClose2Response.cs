using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FindClose2Response : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_FIND_CLOSE2;

	public FindClose2Response()
	{
	}

	public FindClose2Response(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}
}
