using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class AuthenticateMessage
{
	public const string ValidSignature = "NTLMSSP\0";

	public const int MicFieldLenght = 16;

	public string Signature;

	public MessageTypeName MessageType;

	public byte[] LmChallengeResponse;

	public byte[] NtChallengeResponse;

	public string DomainName;

	public string UserName;

	public string WorkStation;

	public byte[] EncryptedRandomSessionKey;

	public NegotiateFlags NegotiateFlags;

	public NTLMVersion Version;

	public byte[] MIC;

	public AuthenticateMessage()
	{
		Signature = "NTLMSSP\0";
		MessageType = MessageTypeName.Authenticate;
		DomainName = string.Empty;
		UserName = string.Empty;
		WorkStation = string.Empty;
		EncryptedRandomSessionKey = new byte[0];
	}

	public AuthenticateMessage(byte[] buffer)
	{
		Signature = ByteReader.ReadAnsiString(buffer, 0, 8);
		MessageType = (MessageTypeName)LittleEndianConverter.ToUInt32(buffer, 8);
		LmChallengeResponse = AuthenticationMessageUtils.ReadBufferPointer(buffer, 12);
		NtChallengeResponse = AuthenticationMessageUtils.ReadBufferPointer(buffer, 20);
		DomainName = AuthenticationMessageUtils.ReadUnicodeStringBufferPointer(buffer, 28);
		UserName = AuthenticationMessageUtils.ReadUnicodeStringBufferPointer(buffer, 36);
		WorkStation = AuthenticationMessageUtils.ReadUnicodeStringBufferPointer(buffer, 44);
		EncryptedRandomSessionKey = AuthenticationMessageUtils.ReadBufferPointer(buffer, 52);
		NegotiateFlags = (NegotiateFlags)LittleEndianConverter.ToUInt32(buffer, 60);
		int num = 64;
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			Version = new NTLMVersion(buffer, num);
			num += 8;
		}
		if (HasMicField())
		{
			MIC = ByteReader.ReadBytes(buffer, num, 16);
		}
	}

	public bool HasMicField()
	{
		if (!AuthenticationMessageUtils.IsNTLMv2NTResponse(NtChallengeResponse))
		{
			return false;
		}
		NTLMv2ClientChallenge nTLMv2ClientChallenge;
		try
		{
			nTLMv2ClientChallenge = new NTLMv2ClientChallenge(NtChallengeResponse, 16);
		}
		catch
		{
			return false;
		}
		int num = nTLMv2ClientChallenge.AVPairs.IndexOfKey(AVPairKey.Flags);
		if (num >= 0)
		{
			byte[] value = nTLMv2ClientChallenge.AVPairs[num].Value;
			if (value.Length == 4)
			{
				return (LittleEndianConverter.ToInt32(value, 0) & 2) > 0;
			}
		}
		return false;
	}

	public byte[] GetBytes()
	{
		if ((NegotiateFlags & NegotiateFlags.KeyExchange) == 0)
		{
			EncryptedRandomSessionKey = new byte[0];
		}
		int num = 64;
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			num += 8;
		}
		if (MIC != null)
		{
			num += MIC.Length;
		}
		int num2 = LmChallengeResponse.Length + NtChallengeResponse.Length + DomainName.Length * 2 + UserName.Length * 2 + WorkStation.Length * 2 + EncryptedRandomSessionKey.Length;
		byte[] array = new byte[num + num2];
		ByteWriter.WriteAnsiString(array, 0, "NTLMSSP\0", 8);
		LittleEndianWriter.WriteUInt32(array, 8, (uint)MessageType);
		LittleEndianWriter.WriteUInt32(array, 60, (uint)NegotiateFlags);
		int offset = 64;
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			Version.WriteBytes(array, offset);
			offset += 8;
		}
		if (MIC != null)
		{
			ByteWriter.WriteBytes(array, offset, MIC);
			offset += MIC.Length;
		}
		AuthenticationMessageUtils.WriteBufferPointer(array, 28, (ushort)(DomainName.Length * 2), (uint)offset);
		ByteWriter.WriteUTF16String(array, ref offset, DomainName);
		AuthenticationMessageUtils.WriteBufferPointer(array, 36, (ushort)(UserName.Length * 2), (uint)offset);
		ByteWriter.WriteUTF16String(array, ref offset, UserName);
		AuthenticationMessageUtils.WriteBufferPointer(array, 44, (ushort)(WorkStation.Length * 2), (uint)offset);
		ByteWriter.WriteUTF16String(array, ref offset, WorkStation);
		AuthenticationMessageUtils.WriteBufferPointer(array, 12, (ushort)LmChallengeResponse.Length, (uint)offset);
		ByteWriter.WriteBytes(array, ref offset, LmChallengeResponse);
		AuthenticationMessageUtils.WriteBufferPointer(array, 20, (ushort)NtChallengeResponse.Length, (uint)offset);
		ByteWriter.WriteBytes(array, ref offset, NtChallengeResponse);
		AuthenticationMessageUtils.WriteBufferPointer(array, 52, (ushort)EncryptedRandomSessionKey.Length, (uint)offset);
		ByteWriter.WriteBytes(array, ref offset, EncryptedRandomSessionKey);
		return array;
	}

	public void CalculateMIC(byte[] sessionKey, byte[] negotiateMessage, byte[] challengeMessage)
	{
		MIC = new byte[16];
		byte[] bytes = GetBytes();
		byte[] buffer = ByteUtils.Concatenate(ByteUtils.Concatenate(negotiateMessage, challengeMessage), bytes);
		MIC = new HMACMD5(sessionKey).ComputeHash(buffer);
	}

	public static int GetMicFieldOffset(byte[] authenticateMessageBytes)
	{
		uint num = LittleEndianConverter.ToUInt32(authenticateMessageBytes, 60);
		int num2 = 64;
		if ((num & 0x2000000u) != 0)
		{
			num2 += 8;
		}
		return num2;
	}
}
