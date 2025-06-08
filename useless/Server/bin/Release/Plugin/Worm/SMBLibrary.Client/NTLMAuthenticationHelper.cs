using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SMBLibrary.Authentication.NTLM;
using Utilities;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class NTLMAuthenticationHelper
{
	public static byte[] GetNegotiateMessage(string domainName, string userName, string password, AuthenticationMethod authenticationMethod)
	{
		NegotiateMessage negotiateMessage = new NegotiateMessage();
		negotiateMessage.NegotiateFlags = NegotiateFlags.UnicodeEncoding | NegotiateFlags.OEMEncoding | NegotiateFlags.Sign | NegotiateFlags.NTLMSessionSecurity | NegotiateFlags.DomainNameSupplied | NegotiateFlags.WorkstationNameSupplied | NegotiateFlags.AlwaysSign | NegotiateFlags.Version | NegotiateFlags.Use128BitEncryption | NegotiateFlags.Use56BitEncryption;
		if (!(userName == string.Empty) || !(password == string.Empty))
		{
			negotiateMessage.NegotiateFlags |= NegotiateFlags.KeyExchange;
		}
		if (authenticationMethod == AuthenticationMethod.NTLMv1)
		{
			negotiateMessage.NegotiateFlags |= NegotiateFlags.LanManagerSessionKey;
		}
		else
		{
			negotiateMessage.NegotiateFlags |= NegotiateFlags.ExtendedSessionSecurity;
		}
		negotiateMessage.Version = NTLMVersion.Server2003;
		negotiateMessage.DomainName = domainName;
		negotiateMessage.Workstation = Environment.MachineName;
		return negotiateMessage.GetBytes();
	}

	public static byte[] GetAuthenticateMessage(byte[] negotiateMessageBytes, byte[] challengeMessageBytes, string domainName, string userName, string password, string spn, AuthenticationMethod authenticationMethod, out byte[] sessionKey)
	{
		sessionKey = null;
		ChallengeMessage challengeMessage = GetChallengeMessage(challengeMessageBytes);
		if (challengeMessage == null)
		{
			return null;
		}
		DateTime utcNow = DateTime.UtcNow;
		byte[] array = new byte[8];
		new Random().NextBytes(array);
		AuthenticateMessage authenticateMessage = new AuthenticateMessage();
		authenticateMessage.NegotiateFlags = NegotiateFlags.Sign | NegotiateFlags.NTLMSessionSecurity | NegotiateFlags.AlwaysSign | NegotiateFlags.Version | NegotiateFlags.Use128BitEncryption | NegotiateFlags.Use56BitEncryption;
		if ((challengeMessage.NegotiateFlags & NegotiateFlags.UnicodeEncoding) != 0)
		{
			authenticateMessage.NegotiateFlags |= NegotiateFlags.UnicodeEncoding;
		}
		else
		{
			authenticateMessage.NegotiateFlags |= NegotiateFlags.OEMEncoding;
		}
		if ((challengeMessage.NegotiateFlags & NegotiateFlags.KeyExchange) != 0)
		{
			authenticateMessage.NegotiateFlags |= NegotiateFlags.KeyExchange;
		}
		if (authenticationMethod == AuthenticationMethod.NTLMv1)
		{
			authenticateMessage.NegotiateFlags |= NegotiateFlags.LanManagerSessionKey;
		}
		else
		{
			authenticateMessage.NegotiateFlags |= NegotiateFlags.ExtendedSessionSecurity;
		}
		if (userName == string.Empty && password == string.Empty)
		{
			authenticateMessage.NegotiateFlags |= NegotiateFlags.Anonymous;
		}
		authenticateMessage.UserName = userName;
		authenticateMessage.DomainName = domainName;
		authenticateMessage.WorkStation = Environment.MachineName;
		byte[] array2;
		if (authenticationMethod == AuthenticationMethod.NTLMv1 || authenticationMethod == AuthenticationMethod.NTLMv1ExtendedSessionSecurity)
		{
			if (userName == string.Empty && password == string.Empty)
			{
				authenticateMessage.LmChallengeResponse = new byte[1];
				authenticateMessage.NtChallengeResponse = new byte[0];
			}
			else if (authenticationMethod == AuthenticationMethod.NTLMv1)
			{
				authenticateMessage.LmChallengeResponse = NTLMCryptography.ComputeLMv1Response(challengeMessage.ServerChallenge, password);
				authenticateMessage.NtChallengeResponse = NTLMCryptography.ComputeNTLMv1Response(challengeMessage.ServerChallenge, password);
			}
			else
			{
				authenticateMessage.LmChallengeResponse = ByteUtils.Concatenate(array, new byte[16]);
				authenticateMessage.NtChallengeResponse = NTLMCryptography.ComputeNTLMv1ExtendedSessionSecurityResponse(challengeMessage.ServerChallenge, array, password);
			}
			array2 = NTLMCryptography.KXKey(new MD4().GetByteHashFromBytes(NTLMCryptography.NTOWFv1(password)), lmowf: NTLMCryptography.LMOWFv1(password), negotiateFlags: authenticateMessage.NegotiateFlags, lmChallengeResponse: authenticateMessage.LmChallengeResponse, serverChallenge: challengeMessage.ServerChallenge);
		}
		else
		{
			byte[] bytesPadded = new NTLMv2ClientChallenge(utcNow, array, challengeMessage.TargetInfo, spn).GetBytesPadded();
			byte[] array3 = NTLMCryptography.ComputeNTLMv2Proof(challengeMessage.ServerChallenge, bytesPadded, password, userName, domainName);
			if (userName == string.Empty && password == string.Empty)
			{
				authenticateMessage.LmChallengeResponse = new byte[1];
				authenticateMessage.NtChallengeResponse = new byte[0];
			}
			else
			{
				authenticateMessage.LmChallengeResponse = NTLMCryptography.ComputeLMv2Response(challengeMessage.ServerChallenge, array, password, userName, challengeMessage.TargetName);
				authenticateMessage.NtChallengeResponse = ByteUtils.Concatenate(array3, bytesPadded);
			}
			array2 = new HMACMD5(NTLMCryptography.NTOWFv2(password, userName, domainName)).ComputeHash(array3);
		}
		authenticateMessage.Version = NTLMVersion.Server2003;
		if ((challengeMessage.NegotiateFlags & NegotiateFlags.KeyExchange) != 0)
		{
			sessionKey = new byte[16];
			new Random().NextBytes(sessionKey);
			authenticateMessage.EncryptedRandomSessionKey = RC4.Encrypt(array2, sessionKey);
		}
		else
		{
			sessionKey = array2;
		}
		authenticateMessage.CalculateMIC(sessionKey, negotiateMessageBytes, challengeMessageBytes);
		return authenticateMessage.GetBytes();
	}

	private static ChallengeMessage GetChallengeMessage(byte[] messageBytes)
	{
		if (AuthenticationMessageUtils.IsSignatureValid(messageBytes) && AuthenticationMessageUtils.GetMessageType(messageBytes) == MessageTypeName.Challenge)
		{
			try
			{
				return new ChallengeMessage(messageBytes);
			}
			catch
			{
				return null;
			}
		}
		return null;
	}
}
