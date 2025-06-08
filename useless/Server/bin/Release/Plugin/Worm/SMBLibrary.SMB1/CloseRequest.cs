using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class CloseRequest : SMB1Command
{
	public const int ParametersLength = 6;

	public ushort FID;

	public DateTime? LastTimeModified;

	public override CommandName CommandName => CommandName.SMB_COM_CLOSE;

	public CloseRequest()
	{
	}

	public CloseRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		LastTimeModified = UTimeHelper.ReadNullableUTime(SMBParameters, 2);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[6];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, FID);
		UTimeHelper.WriteUTime(SMBParameters, 2, LastTimeModified);
		return base.GetBytes(isUnicode);
	}
}
