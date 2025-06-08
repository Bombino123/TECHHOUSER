using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class LogoffAndXResponse : SMBAndXCommand
{
	public const int ParametersLength = 4;

	public override CommandName CommandName => CommandName.SMB_COM_LOGOFF_ANDX;

	public LogoffAndXResponse()
	{
	}

	public LogoffAndXResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[4];
		return base.GetBytes(isUnicode);
	}
}
