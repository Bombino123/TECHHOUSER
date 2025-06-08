using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class EchoResponse : SMB1Command
{
	public const int ParametersLength = 2;

	public ushort SequenceNumber;

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

	public EchoResponse()
	{
	}

	public EchoResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		SequenceNumber = LittleEndianConverter.ToUInt16(SMBParameters, 0);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[2];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, SequenceNumber);
		return base.GetBytes(isUnicode);
	}
}
