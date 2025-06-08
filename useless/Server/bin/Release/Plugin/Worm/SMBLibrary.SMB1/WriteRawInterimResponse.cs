using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class WriteRawInterimResponse : SMB1Command
{
	public const int ParametersLength = 2;

	public ushort Available;

	public override CommandName CommandName => CommandName.SMB_COM_WRITE_RAW;

	public WriteRawInterimResponse()
	{
	}

	public WriteRawInterimResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		Available = LittleEndianConverter.ToUInt16(SMBParameters, 0);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[2];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, Available);
		return base.GetBytes(isUnicode);
	}
}
