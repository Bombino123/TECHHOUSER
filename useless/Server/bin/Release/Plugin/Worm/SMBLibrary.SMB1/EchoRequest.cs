using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class EchoRequest : SMB1Command
{
	public const int ParametersLength = 2;

	public ushort EchoCount;

	public byte[] Data
	{
		get
		{
			return SMBData;
		}
		set
		{
			SMBData = value;
		}
	}

	public override CommandName CommandName => CommandName.SMB_COM_ECHO;

	public EchoRequest()
	{
	}

	public EchoRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		EchoCount = LittleEndianConverter.ToUInt16(SMBParameters, 0);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[2];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, EchoCount);
		return base.GetBytes(isUnicode);
	}
}
