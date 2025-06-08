using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SessionSetupAndXResponse : SMBAndXCommand
{
	public const int ParametersLength = 6;

	public SessionSetupAction Action;

	public string NativeOS;

	public string NativeLanMan;

	public string PrimaryDomain;

	public override CommandName CommandName => CommandName.SMB_COM_SESSION_SETUP_ANDX;

	public SessionSetupAndXResponse()
	{
		NativeOS = string.Empty;
		NativeLanMan = string.Empty;
		PrimaryDomain = string.Empty;
	}

	public SessionSetupAndXResponse(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		Action = (SessionSetupAction)LittleEndianConverter.ToUInt16(SMBParameters, 4);
		int offset2 = 0;
		if (isUnicode)
		{
			offset2++;
		}
		NativeOS = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		NativeLanMan = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		if ((SMBData.Length - offset2) % 2 == 1)
		{
			SMBData = ByteUtils.Concatenate(SMBData, new byte[1]);
		}
		PrimaryDomain = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[6];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, (ushort)Action);
		int offset = 0;
		if (isUnicode)
		{
			int num = 1;
			SMBData = new byte[num + NativeOS.Length * 2 + NativeLanMan.Length * 2 + PrimaryDomain.Length * 2 + 6];
			offset = num;
		}
		else
		{
			SMBData = new byte[NativeOS.Length + NativeLanMan.Length + PrimaryDomain.Length + 3];
		}
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeOS);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeLanMan);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, PrimaryDomain);
		return base.GetBytes(isUnicode);
	}
}
