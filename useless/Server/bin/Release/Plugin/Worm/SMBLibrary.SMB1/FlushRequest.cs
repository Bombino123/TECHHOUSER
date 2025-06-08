using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FlushRequest : SMB1Command
{
	public const int ParametersLength = 2;

	public ushort FID;

	public override CommandName CommandName => CommandName.SMB_COM_FLUSH;

	public FlushRequest()
	{
	}

	public FlushRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 0);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[2];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, FID);
		return base.GetBytes(isUnicode);
	}
}
