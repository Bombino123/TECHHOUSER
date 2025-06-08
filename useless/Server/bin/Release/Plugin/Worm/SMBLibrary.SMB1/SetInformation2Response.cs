using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SetInformation2Response : SMB1Command
{
	public override CommandName CommandName => CommandName.SMB_COM_SET_INFORMATION2;

	public SetInformation2Response()
	{
	}

	public SetInformation2Response(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		return base.GetBytes(isUnicode);
	}
}
