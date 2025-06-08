using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NegotiateResponse : SMB1Command
{
	public const int ParametersLength = 34;

	public ushort DialectIndex;

	public SecurityMode SecurityMode;

	public ushort MaxMpxCount;

	public ushort MaxNumberVcs;

	public uint MaxBufferSize;

	public uint MaxRawSize;

	public uint SessionKey;

	public Capabilities Capabilities;

	public DateTime SystemTime;

	public short ServerTimeZone;

	private byte ChallengeLength;

	public byte[] Challenge;

	public string DomainName;

	public string ServerName;

	public override CommandName CommandName => CommandName.SMB_COM_NEGOTIATE;

	public NegotiateResponse()
	{
		Challenge = new byte[0];
		DomainName = string.Empty;
		ServerName = string.Empty;
	}

	public NegotiateResponse(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		DialectIndex = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		SecurityMode = (SecurityMode)ByteReader.ReadByte(SMBParameters, 2);
		MaxMpxCount = LittleEndianConverter.ToUInt16(SMBParameters, 3);
		MaxNumberVcs = LittleEndianConverter.ToUInt16(SMBParameters, 5);
		MaxBufferSize = LittleEndianConverter.ToUInt32(SMBParameters, 7);
		MaxRawSize = LittleEndianConverter.ToUInt32(SMBParameters, 11);
		SessionKey = LittleEndianConverter.ToUInt32(SMBParameters, 15);
		Capabilities = (Capabilities)LittleEndianConverter.ToUInt32(SMBParameters, 19);
		SystemTime = FileTimeHelper.ReadFileTime(SMBParameters, 23);
		ServerTimeZone = LittleEndianConverter.ToInt16(SMBParameters, 31);
		ChallengeLength = ByteReader.ReadByte(SMBParameters, 33);
		int offset2 = 0;
		Challenge = ByteReader.ReadBytes(SMBData, ref offset2, ChallengeLength);
		DomainName = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode: true);
		ServerName = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode: true);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		ChallengeLength = (byte)Challenge.Length;
		SMBParameters = new byte[34];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, DialectIndex);
		ByteWriter.WriteByte(SMBParameters, 2, (byte)SecurityMode);
		LittleEndianWriter.WriteUInt16(SMBParameters, 3, MaxMpxCount);
		LittleEndianWriter.WriteUInt16(SMBParameters, 5, MaxNumberVcs);
		LittleEndianWriter.WriteUInt32(SMBParameters, 7, MaxBufferSize);
		LittleEndianWriter.WriteUInt32(SMBParameters, 11, MaxRawSize);
		LittleEndianWriter.WriteUInt32(SMBParameters, 15, SessionKey);
		LittleEndianWriter.WriteUInt32(SMBParameters, 19, (uint)Capabilities);
		FileTimeHelper.WriteFileTime(SMBParameters, 23, SystemTime);
		LittleEndianWriter.WriteInt16(SMBParameters, 31, ServerTimeZone);
		ByteWriter.WriteByte(SMBParameters, 33, ChallengeLength);
		SMBData = new byte[Challenge.Length + (DomainName.Length + 1) * 2 + (ServerName.Length + 1) * 2];
		int offset = 0;
		ByteWriter.WriteBytes(SMBData, ref offset, Challenge);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode: true, DomainName);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode: true, ServerName);
		return base.GetBytes(isUnicode);
	}
}
