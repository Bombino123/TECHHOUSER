using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SetInformation2Request : SMB1Command
{
	public const int ParametersLength = 14;

	public ushort FID;

	public DateTime? CreationDateTime;

	public DateTime? LastAccessDateTime;

	public DateTime? LastWriteDateTime;

	public override CommandName CommandName => CommandName.SMB_COM_SET_INFORMATION2;

	public SetInformation2Request()
	{
	}

	public SetInformation2Request(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		CreationDateTime = SMB1Helper.ReadNullableSMBDateTime(SMBParameters, 2);
		LastAccessDateTime = SMB1Helper.ReadNullableSMBDateTime(SMBParameters, 6);
		LastWriteDateTime = SMB1Helper.ReadNullableSMBDateTime(SMBParameters, 10);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[14];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, FID);
		SMB1Helper.WriteSMBDateTime(SMBParameters, 2, CreationDateTime);
		SMB1Helper.WriteSMBDateTime(SMBParameters, 6, LastAccessDateTime);
		SMB1Helper.WriteSMBDateTime(SMBParameters, 10, LastWriteDateTime);
		return base.GetBytes(isUnicode);
	}
}
