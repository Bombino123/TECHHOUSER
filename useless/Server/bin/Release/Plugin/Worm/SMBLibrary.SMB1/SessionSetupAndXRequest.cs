using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SessionSetupAndXRequest : SMBAndXCommand
{
	public const int ParametersLength = 26;

	public ushort MaxBufferSize;

	public ushort MaxMpxCount;

	public ushort VcNumber;

	public uint SessionKey;

	private ushort OEMPasswordLength;

	private ushort UnicodePasswordLength;

	public uint Reserved;

	public Capabilities Capabilities;

	public byte[] OEMPassword;

	public byte[] UnicodePassword;

	public string AccountName;

	public string PrimaryDomain;

	public string NativeOS;

	public string NativeLanMan;

	public override CommandName CommandName => CommandName.SMB_COM_SESSION_SETUP_ANDX;

	public SessionSetupAndXRequest()
	{
		AccountName = string.Empty;
		PrimaryDomain = string.Empty;
		NativeOS = string.Empty;
		NativeLanMan = string.Empty;
	}

	public SessionSetupAndXRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		MaxBufferSize = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		MaxMpxCount = LittleEndianConverter.ToUInt16(SMBParameters, 6);
		VcNumber = LittleEndianConverter.ToUInt16(SMBParameters, 8);
		SessionKey = LittleEndianConverter.ToUInt32(SMBParameters, 10);
		OEMPasswordLength = LittleEndianConverter.ToUInt16(SMBParameters, 14);
		UnicodePasswordLength = LittleEndianConverter.ToUInt16(SMBParameters, 16);
		Reserved = LittleEndianConverter.ToUInt32(SMBParameters, 18);
		Capabilities = (Capabilities)LittleEndianConverter.ToUInt32(SMBParameters, 22);
		OEMPassword = ByteReader.ReadBytes(SMBData, 0, OEMPasswordLength);
		UnicodePassword = ByteReader.ReadBytes(SMBData, OEMPasswordLength, UnicodePasswordLength);
		int offset2 = OEMPasswordLength + UnicodePasswordLength;
		if (isUnicode)
		{
			int num = (1 + OEMPasswordLength + UnicodePasswordLength) % 2;
			offset2 += num;
		}
		AccountName = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		PrimaryDomain = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		NativeOS = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		NativeLanMan = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		Capabilities &= ~Capabilities.ExtendedSecurity;
		OEMPasswordLength = (ushort)OEMPassword.Length;
		UnicodePasswordLength = (ushort)UnicodePassword.Length;
		SMBParameters = new byte[26];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, MaxBufferSize);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, MaxMpxCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 8, VcNumber);
		LittleEndianWriter.WriteUInt32(SMBParameters, 10, SessionKey);
		LittleEndianWriter.WriteUInt16(SMBParameters, 14, OEMPasswordLength);
		LittleEndianWriter.WriteUInt16(SMBParameters, 16, UnicodePasswordLength);
		LittleEndianWriter.WriteUInt32(SMBParameters, 18, Reserved);
		LittleEndianWriter.WriteUInt32(SMBParameters, 22, (uint)Capabilities);
		int num = 0;
		if (isUnicode)
		{
			num = (1 + OEMPasswordLength + UnicodePasswordLength) % 2;
			SMBData = new byte[OEMPassword.Length + UnicodePassword.Length + num + (AccountName.Length + 1) * 2 + (PrimaryDomain.Length + 1) * 2 + (NativeOS.Length + 1) * 2 + (NativeLanMan.Length + 1) * 2];
		}
		else
		{
			SMBData = new byte[OEMPassword.Length + UnicodePassword.Length + AccountName.Length + 1 + PrimaryDomain.Length + 1 + NativeOS.Length + 1 + NativeLanMan.Length + 1];
		}
		int offset = 0;
		ByteWriter.WriteBytes(SMBData, ref offset, OEMPassword);
		ByteWriter.WriteBytes(SMBData, ref offset, UnicodePassword);
		offset += num;
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, AccountName);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, PrimaryDomain);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeOS);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeLanMan);
		return base.GetBytes(isUnicode);
	}
}
