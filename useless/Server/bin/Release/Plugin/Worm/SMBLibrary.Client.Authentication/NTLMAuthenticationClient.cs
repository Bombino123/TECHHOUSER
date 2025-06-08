using System.Collections.Generic;
using System.Runtime.InteropServices;
using SMBLibrary.Authentication.GSSAPI;
using Utilities;

namespace SMBLibrary.Client.Authentication;

[ComVisible(true)]
public class NTLMAuthenticationClient : IAuthenticationClient
{
	private string m_domainName;

	private string m_userName;

	private string m_password;

	private string m_spn;

	private byte[] m_sessionKey;

	private AuthenticationMethod m_authenticationMethod;

	private byte[] m_negotiateMessageBytes;

	private bool m_isNegotiationMessageAcquired;

	public NTLMAuthenticationClient(string domainName, string userName, string password, string spn, AuthenticationMethod authenticationMethod)
	{
		m_domainName = domainName;
		m_userName = userName;
		m_password = password;
		m_spn = spn;
		m_authenticationMethod = authenticationMethod;
	}

	public byte[] InitializeSecurityContext(byte[] securityBlob)
	{
		if (!m_isNegotiationMessageAcquired)
		{
			m_isNegotiationMessageAcquired = true;
			return GetNegotiateMessage(securityBlob);
		}
		return GetAuthenticateMessage(securityBlob);
	}

	protected virtual byte[] GetNegotiateMessage(byte[] securityBlob)
	{
		bool flag = false;
		if (securityBlob.Length != 0)
		{
			SimpleProtectedNegotiationTokenInit simpleProtectedNegotiationTokenInit = null;
			try
			{
				simpleProtectedNegotiationTokenInit = SimpleProtectedNegotiationToken.ReadToken(securityBlob, 0, serverInitiatedNegotiation: true) as SimpleProtectedNegotiationTokenInit;
			}
			catch
			{
			}
			if (simpleProtectedNegotiationTokenInit == null || !ContainsMechanism(simpleProtectedNegotiationTokenInit, GSSProvider.NTLMSSPIdentifier))
			{
				return null;
			}
			flag = true;
		}
		m_negotiateMessageBytes = NTLMAuthenticationHelper.GetNegotiateMessage(m_domainName, m_userName, m_password, m_authenticationMethod);
		if (flag)
		{
			SimpleProtectedNegotiationTokenInit simpleProtectedNegotiationTokenInit2 = new SimpleProtectedNegotiationTokenInit();
			simpleProtectedNegotiationTokenInit2.MechanismTypeList = new List<byte[]>();
			simpleProtectedNegotiationTokenInit2.MechanismTypeList.Add(GSSProvider.NTLMSSPIdentifier);
			simpleProtectedNegotiationTokenInit2.MechanismToken = m_negotiateMessageBytes;
			return simpleProtectedNegotiationTokenInit2.GetBytes(includeHeader: true);
		}
		return m_negotiateMessageBytes;
	}

	protected virtual byte[] GetAuthenticateMessage(byte[] securityBlob)
	{
		bool flag = false;
		SimpleProtectedNegotiationTokenResponse simpleProtectedNegotiationTokenResponse = null;
		try
		{
			simpleProtectedNegotiationTokenResponse = SimpleProtectedNegotiationToken.ReadToken(securityBlob, 0, serverInitiatedNegotiation: false) as SimpleProtectedNegotiationTokenResponse;
		}
		catch
		{
		}
		byte[] challengeMessageBytes;
		if (simpleProtectedNegotiationTokenResponse != null)
		{
			challengeMessageBytes = simpleProtectedNegotiationTokenResponse.ResponseToken;
			flag = true;
		}
		else
		{
			challengeMessageBytes = securityBlob;
		}
		byte[] authenticateMessage = NTLMAuthenticationHelper.GetAuthenticateMessage(m_negotiateMessageBytes, challengeMessageBytes, m_domainName, m_userName, m_password, m_spn, m_authenticationMethod, out m_sessionKey);
		if (flag && authenticateMessage != null)
		{
			return new SimpleProtectedNegotiationTokenResponse
			{
				ResponseToken = authenticateMessage
			}.GetBytes();
		}
		return authenticateMessage;
	}

	public virtual byte[] GetSessionKey()
	{
		return m_sessionKey;
	}

	private static bool ContainsMechanism(SimpleProtectedNegotiationTokenInit token, byte[] mechanismIdentifier)
	{
		for (int i = 0; i < token.MechanismTypeList.Count; i++)
		{
			if (ByteUtils.AreByteArraysEqual(token.MechanismTypeList[i], mechanismIdentifier))
			{
				return true;
			}
		}
		return false;
	}
}
