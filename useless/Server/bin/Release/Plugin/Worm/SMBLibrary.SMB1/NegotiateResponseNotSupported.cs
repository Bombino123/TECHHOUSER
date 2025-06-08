using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NegotiateResponseNotSupported : SMB1Command
{
	public const int ParametersLength = 2;

	public const ushort DialectsNotSupported = ushort.MaxValue;

	public override CommandName CommandName => CommandName.SMB_COM_NEGOTIATE;

	public NegotiateResponseNotSupported()
	{
	}

	public NegotiateResponseNotSupported(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		throw new NotImplementedException();
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[2];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, ushort.MaxValue);
		SMBData = new byte[0];
		return base.GetBytes(isUnicode);
	}
}
