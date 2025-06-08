using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class LockingAndXResponse : SMBAndXCommand
{
	public const int ParametersLength = 4;

	public override CommandName CommandName => CommandName.SMB_COM_LOCKING_ANDX;

	public LockingAndXResponse()
	{
	}

	public LockingAndXResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[4];
		return base.GetBytes(isUnicode);
	}
}
