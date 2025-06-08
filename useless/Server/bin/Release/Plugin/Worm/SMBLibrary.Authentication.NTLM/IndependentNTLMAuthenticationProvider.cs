using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SMBLibrary.Authentication.GSSAPI;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class IndependentNTLMAuthenticationProvider : NTLMAuthenticationProviderBase
{
	public class AuthContext
	{
		public byte[] ServerChallenge;

		public string DomainName;

		public string UserName;

		public string WorkStation;

		public string OSVersion;

		public byte[] SessionKey;

		public bool IsGuest;

		public AuthContext(byte[] serverChallenge)
		{
			ServerChallenge = serverChallenge;
		}
	}

	private static readonly int DefaultMaxLoginAttemptsInWindow = 100;

	private static readonly TimeSpan DefaultLoginWindowDuration = new TimeSpan(0, 20, 0);

	private GetUserPassword m_GetUserPassword;

	private LoginCounter m_loginCounter;

	private bool EnableGuestLogin => m_GetUserPassword("Guest") == string.Empty;

	public IndependentNTLMAuthenticationProvider(GetUserPassword getUserPassword)
		: this(getUserPassword, DefaultMaxLoginAttemptsInWindow, DefaultLoginWindowDuration)
	{
	}

	public IndependentNTLMAuthenticationProvider(GetUserPassword getUserPassword, int maxLoginAttemptsInWindow, TimeSpan loginWindowDuration)
	{
		m_GetUserPassword = getUserPassword;
		m_loginCounter = new LoginCounter(maxLoginAttemptsInWindow, loginWindowDuration);
	}

	public override NTStatus GetChallengeMessage(out object context, byte[] negotiateMessageBytes, out byte[] challengeMessageBytes)
	{
		NegotiateMessage negotiateMessage;
		try
		{
			negotiateMessage = new NegotiateMessage(negotiateMessageBytes);
		}
		catch
		{
			context = null;
			challengeMessageBytes = null;
			return NTStatus.SEC_E_INVALID_TOKEN;
		}
		byte[] serverChallenge = GenerateServerChallenge();
		context = new AuthContext(serverChallenge);
		ChallengeMessage challengeMessage = new ChallengeMessage();
		challengeMessage.NegotiateFlags = NegotiateFlags.TargetNameSupplied | NegotiateFlags.TargetTypeServer | NegotiateFlags.TargetInfo | NegotiateFlags.Version;
		challengeMessage.NegotiateFlags |= NegotiateFlags.NTLMSessionSecurity;
		if ((negotiateMessage.NegotiateFlags & NegotiateFlags.UnicodeEncoding) != 0)
		{
			challengeMessage.NegotiateFlags |= NegotiateFlags.UnicodeEncoding;
		}
		else if ((negotiateMessage.NegotiateFlags & NegotiateFlags.OEMEncoding) != 0)
		{
			challengeMessage.NegotiateFlags |= NegotiateFlags.OEMEncoding;
		}
		if ((negotiateMessage.NegotiateFlags & NegotiateFlags.ExtendedSessionSecurity) != 0)
		{
			challengeMessage.NegotiateFlags |= NegotiateFlags.ExtendedSessionSecurity;
		}
		else if ((negotiateMessage.NegotiateFlags & NegotiateFlags.LanManagerSessionKey) != 0)
		{
			challengeMessage.NegotiateFlags |= NegotiateFlags.LanManagerSessionKey;
		}
		if ((negotiateMessage.NegotiateFlags & NegotiateFlags.Sign) != 0)
		{
			challengeMessage.NegotiateFlags |= NegotiateFlags.Sign;
		}
		if ((negotiateMessage.NegotiateFlags & NegotiateFlags.Seal) != 0)
		{
			challengeMessage.NegotiateFlags |= NegotiateFlags.Seal;
		}
		if ((negotiateMessage.NegotiateFlags & NegotiateFlags.Sign) != 0 || (negotiateMessage.NegotiateFlags & NegotiateFlags.Seal) != 0)
		{
			if ((negotiateMessage.NegotiateFlags & NegotiateFlags.Use56BitEncryption) != 0)
			{
				challengeMessage.NegotiateFlags |= NegotiateFlags.Use56BitEncryption;
			}
			if ((negotiateMessage.NegotiateFlags & NegotiateFlags.Use128BitEncryption) != 0)
			{
				challengeMessage.NegotiateFlags |= NegotiateFlags.Use128BitEncryption;
			}
		}
		if ((negotiateMessage.NegotiateFlags & NegotiateFlags.KeyExchange) != 0)
		{
			challengeMessage.NegotiateFlags |= NegotiateFlags.KeyExchange;
		}
		challengeMessage.TargetName = Environment.MachineName;
		challengeMessage.ServerChallenge = serverChallenge;
		challengeMessage.TargetInfo = AVPairUtils.GetAVPairSequence(Environment.MachineName, Environment.MachineName);
		challengeMessage.Version = NTLMVersion.Server2003;
		challengeMessageBytes = challengeMessage.GetBytes();
		return NTStatus.SEC_I_CONTINUE_NEEDED;
	}

	public override NTStatus Authenticate(object context, byte[] authenticateMessageBytes)
	{
		AuthenticateMessage authenticateMessage;
		try
		{
			authenticateMessage = new AuthenticateMessage(authenticateMessageBytes);
		}
		catch
		{
			return NTStatus.SEC_E_INVALID_TOKEN;
		}
		if (!(context is AuthContext authContext))
		{
			return NTStatus.SEC_E_INVALID_TOKEN;
		}
		authContext.DomainName = authenticateMessage.DomainName;
		authContext.UserName = authenticateMessage.UserName;
		authContext.WorkStation = authenticateMessage.WorkStation;
		if (authenticateMessage.Version != null)
		{
			authContext.OSVersion = authenticateMessage.Version.ToString();
		}
		if ((authenticateMessage.NegotiateFlags & NegotiateFlags.Anonymous) != 0)
		{
			if (EnableGuestLogin)
			{
				authContext.IsGuest = true;
				return NTStatus.STATUS_SUCCESS;
			}
			return NTStatus.STATUS_LOGON_FAILURE;
		}
		if (!m_loginCounter.HasRemainingLoginAttempts(authenticateMessage.UserName.ToLower()))
		{
			return NTStatus.STATUS_ACCOUNT_LOCKED_OUT;
		}
		string text = m_GetUserPassword(authenticateMessage.UserName);
		if (text == null)
		{
			if (EnableGuestLogin)
			{
				authContext.IsGuest = true;
				return NTStatus.STATUS_SUCCESS;
			}
			if (m_loginCounter.HasRemainingLoginAttempts(authenticateMessage.UserName.ToLower(), incrementCount: true))
			{
				return NTStatus.STATUS_LOGON_FAILURE;
			}
			return NTStatus.STATUS_ACCOUNT_LOCKED_OUT;
		}
		byte[] serverChallenge = authContext.ServerChallenge;
		byte[] array = null;
		bool flag;
		if ((authenticateMessage.NegotiateFlags & NegotiateFlags.ExtendedSessionSecurity) != 0)
		{
			if (AuthenticationMessageUtils.IsNTLMv1ExtendedSessionSecurity(authenticateMessage.LmChallengeResponse))
			{
				flag = AuthenticateV1Extended(text, serverChallenge, authenticateMessage.LmChallengeResponse, authenticateMessage.NtChallengeResponse);
				if (flag)
				{
					array = NTLMCryptography.KXKey(new MD4().GetByteHashFromBytes(NTLMCryptography.NTOWFv1(text)), lmowf: NTLMCryptography.LMOWFv1(text), negotiateFlags: authenticateMessage.NegotiateFlags, lmChallengeResponse: authenticateMessage.LmChallengeResponse, serverChallenge: serverChallenge);
				}
			}
			else
			{
				flag = AuthenticateV2(authenticateMessage.DomainName, authenticateMessage.UserName, text, serverChallenge, authenticateMessage.LmChallengeResponse, authenticateMessage.NtChallengeResponse);
				if (flag)
				{
					byte[] key = NTLMCryptography.NTOWFv2(text, authenticateMessage.UserName, authenticateMessage.DomainName);
					byte[] buffer = ByteReader.ReadBytes(authenticateMessage.NtChallengeResponse, 0, 16);
					array = new HMACMD5(key).ComputeHash(buffer);
				}
			}
		}
		else
		{
			flag = AuthenticateV1(text, serverChallenge, authenticateMessage.LmChallengeResponse, authenticateMessage.NtChallengeResponse);
			if (flag)
			{
				array = NTLMCryptography.KXKey(new MD4().GetByteHashFromBytes(NTLMCryptography.NTOWFv1(text)), lmowf: NTLMCryptography.LMOWFv1(text), negotiateFlags: authenticateMessage.NegotiateFlags, lmChallengeResponse: authenticateMessage.LmChallengeResponse, serverChallenge: serverChallenge);
			}
		}
		if (flag)
		{
			if ((authenticateMessage.NegotiateFlags & NegotiateFlags.KeyExchange) != 0)
			{
				authContext.SessionKey = RC4.Decrypt(array, authenticateMessage.EncryptedRandomSessionKey);
			}
			else
			{
				authContext.SessionKey = array;
			}
			return NTStatus.STATUS_SUCCESS;
		}
		if (m_loginCounter.HasRemainingLoginAttempts(authenticateMessage.UserName.ToLower(), incrementCount: true))
		{
			return NTStatus.STATUS_LOGON_FAILURE;
		}
		return NTStatus.STATUS_ACCOUNT_LOCKED_OUT;
	}

	public override bool DeleteSecurityContext(ref object context)
	{
		context = null;
		return true;
	}

	public override object GetContextAttribute(object context, GSSAttributeName attributeName)
	{
		if (context is AuthContext authContext)
		{
			switch (attributeName)
			{
			case GSSAttributeName.DomainName:
				return authContext.DomainName;
			case GSSAttributeName.IsGuest:
				return authContext.IsGuest;
			case GSSAttributeName.MachineName:
				return authContext.WorkStation;
			case GSSAttributeName.OSVersion:
				return authContext.OSVersion;
			case GSSAttributeName.SessionKey:
				return authContext.SessionKey;
			case GSSAttributeName.UserName:
				return authContext.UserName;
			}
		}
		return null;
	}

	private static bool AuthenticateV1(string password, byte[] serverChallenge, byte[] lmResponse, byte[] ntResponse)
	{
		if (ByteUtils.AreByteArraysEqual(NTLMCryptography.ComputeLMv1Response(serverChallenge, password), lmResponse))
		{
			return true;
		}
		return ByteUtils.AreByteArraysEqual(NTLMCryptography.ComputeNTLMv1Response(serverChallenge, password), ntResponse);
	}

	private static bool AuthenticateV1Extended(string password, byte[] serverChallenge, byte[] lmResponse, byte[] ntResponse)
	{
		byte[] clientChallenge = ByteReader.ReadBytes(lmResponse, 0, 8);
		return ByteUtils.AreByteArraysEqual(NTLMCryptography.ComputeNTLMv1ExtendedSessionSecurityResponse(serverChallenge, clientChallenge, password), ntResponse);
	}

	private bool AuthenticateV2(string domainName, string accountName, string password, byte[] serverChallenge, byte[] lmResponse, byte[] ntResponse)
	{
		if (lmResponse.Length == 24)
		{
			byte[] clientChallenge = ByteReader.ReadBytes(lmResponse, 16, 8);
			if (ByteUtils.AreByteArraysEqual(NTLMCryptography.ComputeLMv2Response(serverChallenge, clientChallenge, password, accountName, domainName), lmResponse))
			{
				return true;
			}
		}
		if (AuthenticationMessageUtils.IsNTLMv2NTResponse(ntResponse))
		{
			byte[] array = ByteReader.ReadBytes(ntResponse, 0, 16);
			byte[] clientChallengeStructurePadded = ByteReader.ReadBytes(ntResponse, 16, ntResponse.Length - 16);
			byte[] array2 = NTLMCryptography.ComputeNTLMv2Proof(serverChallenge, clientChallengeStructurePadded, password, accountName, domainName);
			return ByteUtils.AreByteArraysEqual(array, array2);
		}
		return false;
	}

	private static byte[] GenerateServerChallenge()
	{
		byte[] array = new byte[8];
		new Random().NextBytes(array);
		return array;
	}
}
