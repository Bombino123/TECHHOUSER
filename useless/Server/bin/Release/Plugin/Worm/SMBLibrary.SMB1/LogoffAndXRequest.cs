using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class LogoffAndXRequest : SMBAndXCommand
{
	public const int ParametersLength = 4;

	public override CommandName CommandName => CommandName.SMB_COM_LOGOFF_ANDX;

	public LogoffAndXRequest()
	{
	}

	public LogoffAndXRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[4];
		return base.GetBytes(isUnicode);
	}
}
