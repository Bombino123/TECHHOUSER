using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class WriteRawFinalResponse : SMB1Command
{
	public const int ParametersLength = 2;

	public ushort Count;

	public override CommandName CommandName => CommandName.SMB_COM_WRITE_COMPLETE;

	public WriteRawFinalResponse()
	{
	}

	public WriteRawFinalResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		Count = LittleEndianConverter.ToUInt16(SMBParameters, 0);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[2];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, Count);
		return base.GetBytes(isUnicode);
	}
}
