using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SessionSetupAndXRequestExtended : SMBAndXCommand
{
	public const int ParametersLength = 24;

	public ushort MaxBufferSize;

	public ushort MaxMpxCount;

	public ushort VcNumber;

	public uint SessionKey;

	private ushort SecurityBlobLength;

	public uint Reserved;

	public Capabilities Capabilities;

	public byte[] SecurityBlob;

	public string NativeOS;

	public string NativeLanMan;

	public override CommandName CommandName => CommandName.SMB_COM_SESSION_SETUP_ANDX;

	public SessionSetupAndXRequestExtended()
	{
		NativeOS = string.Empty;
		NativeLanMan = string.Empty;
	}

	public SessionSetupAndXRequestExtended(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		MaxBufferSize = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		MaxMpxCount = LittleEndianConverter.ToUInt16(SMBParameters, 6);
		VcNumber = LittleEndianConverter.ToUInt16(SMBParameters, 8);
		SessionKey = LittleEndianConverter.ToUInt32(SMBParameters, 10);
		SecurityBlobLength = LittleEndianConverter.ToUInt16(SMBParameters, 14);
		Reserved = LittleEndianConverter.ToUInt32(SMBParameters, 16);
		Capabilities = (Capabilities)LittleEndianConverter.ToUInt32(SMBParameters, 20);
		SecurityBlob = ByteReader.ReadBytes(SMBData, 0, SecurityBlobLength);
		int offset2 = SecurityBlob.Length;
		if (isUnicode)
		{
			int num = (1 + SecurityBlobLength) % 2;
			offset2 += num;
		}
		NativeOS = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		NativeLanMan = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		Capabilities |= Capabilities.ExtendedSecurity;
		SecurityBlobLength = (ushort)SecurityBlob.Length;
		SMBParameters = new byte[24];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, MaxBufferSize);
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, MaxMpxCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 8, VcNumber);
		LittleEndianWriter.WriteUInt32(SMBParameters, 10, SessionKey);
		LittleEndianWriter.WriteUInt16(SMBParameters, 14, SecurityBlobLength);
		LittleEndianWriter.WriteUInt32(SMBParameters, 16, Reserved);
		LittleEndianWriter.WriteUInt32(SMBParameters, 20, (uint)Capabilities);
		int num = 0;
		if (isUnicode)
		{
			num = (1 + SecurityBlobLength) % 2;
			SMBData = new byte[SecurityBlob.Length + num + (NativeOS.Length + 1) * 2 + (NativeLanMan.Length + 1) * 2];
		}
		else
		{
			SMBData = new byte[SecurityBlob.Length + NativeOS.Length + 1 + NativeLanMan.Length + 1];
		}
		int offset = 0;
		ByteWriter.WriteBytes(SMBData, ref offset, SecurityBlob);
		offset += num;
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeOS);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeLanMan);
		return base.GetBytes(isUnicode);
	}
}
