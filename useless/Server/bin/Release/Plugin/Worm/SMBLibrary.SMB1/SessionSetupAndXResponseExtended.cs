using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SessionSetupAndXResponseExtended : SMBAndXCommand
{
	public const int ParametersLength = 8;

	public SessionSetupAction Action;

	private ushort SecurityBlobLength;

	public byte[] SecurityBlob;

	public string NativeOS;

	public string NativeLanMan;

	public override CommandName CommandName => CommandName.SMB_COM_SESSION_SETUP_ANDX;

	public SessionSetupAndXResponseExtended()
	{
		SecurityBlob = new byte[0];
		NativeOS = string.Empty;
		NativeLanMan = string.Empty;
	}

	public SessionSetupAndXResponseExtended(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		Action = (SessionSetupAction)LittleEndianConverter.ToUInt16(SMBParameters, 4);
		SecurityBlobLength = LittleEndianConverter.ToUInt16(SMBParameters, 6);
		SecurityBlob = ByteReader.ReadBytes(SMBData, 0, SecurityBlobLength);
		int offset2 = SecurityBlob.Length;
		if (isUnicode)
		{
			int num = (1 + SecurityBlobLength) % 2;
			offset2 += num;
		}
		NativeOS = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		if ((SMBData.Length - offset2) % 2 == 1)
		{
			SMBData = ByteUtils.Concatenate(SMBData, new byte[1]);
		}
		NativeLanMan = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		ushort num = (ushort)SecurityBlob.Length;
		SMBParameters = new byte[8];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, (ushort)Action);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, num);
		int num2 = 0;
		if (isUnicode)
		{
			num2 = (1 + num) % 2;
			SMBData = new byte[SecurityBlob.Length + num2 + NativeOS.Length * 2 + NativeLanMan.Length * 2 + 4];
		}
		else
		{
			SMBData = new byte[SecurityBlob.Length + NativeOS.Length + NativeLanMan.Length + 2];
		}
		int offset = 0;
		ByteWriter.WriteBytes(SMBData, ref offset, SecurityBlob);
		offset += num2;
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeOS);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeLanMan);
		return base.GetBytes(isUnicode);
	}
}
