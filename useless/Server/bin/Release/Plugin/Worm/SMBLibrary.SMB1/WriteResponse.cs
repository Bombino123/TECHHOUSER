using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class WriteResponse : SMB1Command
{
	public const int ParametersLength = 2;

	public ushort CountOfBytesWritten;

	public override CommandName CommandName => CommandName.SMB_COM_WRITE;

	public WriteResponse()
	{
	}

	public WriteResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		CountOfBytesWritten = LittleEndianConverter.ToUInt16(SMBParameters, 0);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[2];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, CountOfBytesWritten);
		return base.GetBytes(isUnicode);
	}
}
