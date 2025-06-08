using System.Collections.Generic;
using System.Runtime.InteropServices;
using SMBLibrary.Authentication.NTLM;
using Utilities;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public class GSSProvider
{
	public static readonly byte[] NTLMSSPIdentifier = new byte[10] { 43, 6, 1, 4, 1, 130, 55, 2, 2, 10 };

	private List<IGSSMechanism> m_mechanisms;

	public GSSProvider(IGSSMechanism mechanism)
	{
		m_mechanisms = new List<IGSSMechanism>();
		m_mechanisms.Add(mechanism);
	}

	public GSSProvider(List<IGSSMechanism> mechanisms)
	{
		m_mechanisms = mechanisms;
	}

	public byte[] GetSPNEGOTokenInitBytes()
	{
		SimpleProtectedNegotiationTokenInit simpleProtectedNegotiationTokenInit = new SimpleProtectedNegotiationTokenInit();
		simpleProtectedNegotiationTokenInit.MechanismTypeList = new List<byte[]>();
		foreach (IGSSMechanism mechanism in m_mechanisms)
		{
			simpleProtectedNegotiationTokenInit.MechanismTypeList.Add(mechanism.Identifier);
		}
		return simpleProtectedNegotiationTokenInit.GetBytes(includeHeader: true);
	}

	public virtual NTStatus AcceptSecurityContext(ref GSSContext context, byte[] inputToken, out byte[] outputToken)
	{
		outputToken = null;
		SimpleProtectedNegotiationToken simpleProtectedNegotiationToken = null;
		try
		{
			simpleProtectedNegotiationToken = SimpleProtectedNegotiationToken.ReadToken(inputToken, 0, serverInitiatedNegotiation: false);
		}
		catch
		{
		}
		if (simpleProtectedNegotiationToken != null)
		{
			if (simpleProtectedNegotiationToken is SimpleProtectedNegotiationTokenInit)
			{
				SimpleProtectedNegotiationTokenInit simpleProtectedNegotiationTokenInit = (SimpleProtectedNegotiationTokenInit)simpleProtectedNegotiationToken;
				if (simpleProtectedNegotiationTokenInit.MechanismTypeList.Count == 0)
				{
					return NTStatus.SEC_E_INVALID_TOKEN;
				}
				byte[] mechanismIdentifier = simpleProtectedNegotiationTokenInit.MechanismTypeList[0];
				IGSSMechanism iGSSMechanism = FindMechanism(mechanismIdentifier);
				bool flag = iGSSMechanism != null;
				if (!flag)
				{
					iGSSMechanism = FindMechanism(simpleProtectedNegotiationTokenInit.MechanismTypeList);
				}
				if (iGSSMechanism != null)
				{
					context = new GSSContext(iGSSMechanism, null);
					NTStatus nTStatus;
					if (flag)
					{
						nTStatus = iGSSMechanism.AcceptSecurityContext(ref context.MechanismContext, simpleProtectedNegotiationTokenInit.MechanismToken, out var outputToken2);
						outputToken = GetSPNEGOTokenResponseBytes(outputToken2, nTStatus, iGSSMechanism.Identifier);
					}
					else
					{
						nTStatus = NTStatus.SEC_I_CONTINUE_NEEDED;
						outputToken = GetSPNEGOTokenResponseBytes(null, nTStatus, iGSSMechanism.Identifier);
					}
					return nTStatus;
				}
				return NTStatus.SEC_E_SECPKG_NOT_FOUND;
			}
			if (context == null)
			{
				return NTStatus.SEC_E_INVALID_TOKEN;
			}
			byte[] outputToken3;
			NTStatus nTStatus2 = context.Mechanism.AcceptSecurityContext(inputToken: ((SimpleProtectedNegotiationTokenResponse)simpleProtectedNegotiationToken).ResponseToken, context: ref context.MechanismContext, outputToken: out outputToken3);
			outputToken = GetSPNEGOTokenResponseBytes(outputToken3, nTStatus2, null);
			return nTStatus2;
		}
		if (AuthenticationMessageUtils.IsSignatureValid(inputToken))
		{
			MessageTypeName messageType = AuthenticationMessageUtils.GetMessageType(inputToken);
			IGSSMechanism iGSSMechanism2 = FindMechanism(NTLMSSPIdentifier);
			if (iGSSMechanism2 != null)
			{
				if (messageType == MessageTypeName.Negotiate)
				{
					context = new GSSContext(iGSSMechanism2, null);
				}
				if (context == null)
				{
					return NTStatus.SEC_E_INVALID_TOKEN;
				}
				return iGSSMechanism2.AcceptSecurityContext(ref context.MechanismContext, inputToken, out outputToken);
			}
			return NTStatus.SEC_E_SECPKG_NOT_FOUND;
		}
		return NTStatus.SEC_E_INVALID_TOKEN;
	}

	public virtual object GetContextAttribute(GSSContext context, GSSAttributeName attributeName)
	{
		return context?.Mechanism.GetContextAttribute(context.MechanismContext, attributeName);
	}

	public virtual bool DeleteSecurityContext(ref GSSContext context)
	{
		if (context != null)
		{
			return context.Mechanism.DeleteSecurityContext(ref context.MechanismContext);
		}
		return false;
	}

	public virtual NTStatus GetNTLMChallengeMessage(out GSSContext context, NegotiateMessage negotiateMessage, out ChallengeMessage challengeMessage)
	{
		IGSSMechanism iGSSMechanism = FindMechanism(NTLMSSPIdentifier);
		if (iGSSMechanism != null)
		{
			context = new GSSContext(iGSSMechanism, null);
			byte[] outputToken;
			NTStatus result = iGSSMechanism.AcceptSecurityContext(ref context.MechanismContext, negotiateMessage.GetBytes(), out outputToken);
			challengeMessage = new ChallengeMessage(outputToken);
			return result;
		}
		context = null;
		challengeMessage = null;
		return NTStatus.SEC_E_SECPKG_NOT_FOUND;
	}

	public virtual NTStatus NTLMAuthenticate(GSSContext context, AuthenticateMessage authenticateMessage)
	{
		byte[] outputToken;
		if (context != null && ByteUtils.AreByteArraysEqual(context.Mechanism.Identifier, NTLMSSPIdentifier))
		{
			return context.Mechanism.AcceptSecurityContext(ref context.MechanismContext, authenticateMessage.GetBytes(), out outputToken);
		}
		return NTStatus.SEC_E_SECPKG_NOT_FOUND;
	}

	public IGSSMechanism FindMechanism(List<byte[]> mechanismIdentifiers)
	{
		foreach (byte[] mechanismIdentifier in mechanismIdentifiers)
		{
			IGSSMechanism iGSSMechanism = FindMechanism(mechanismIdentifier);
			if (iGSSMechanism != null)
			{
				return iGSSMechanism;
			}
		}
		return null;
	}

	public IGSSMechanism FindMechanism(byte[] mechanismIdentifier)
	{
		foreach (IGSSMechanism mechanism in m_mechanisms)
		{
			if (ByteUtils.AreByteArraysEqual(mechanism.Identifier, mechanismIdentifier))
			{
				return mechanism;
			}
		}
		return null;
	}

	private static byte[] GetSPNEGOTokenResponseBytes(byte[] mechanismOutput, NTStatus status, byte[] mechanismIdentifier)
	{
		SimpleProtectedNegotiationTokenResponse simpleProtectedNegotiationTokenResponse = new SimpleProtectedNegotiationTokenResponse();
		switch (status)
		{
		case NTStatus.STATUS_SUCCESS:
			simpleProtectedNegotiationTokenResponse.NegState = NegState.AcceptCompleted;
			break;
		case NTStatus.SEC_I_CONTINUE_NEEDED:
			simpleProtectedNegotiationTokenResponse.NegState = NegState.AcceptIncomplete;
			break;
		default:
			simpleProtectedNegotiationTokenResponse.NegState = NegState.Reject;
			break;
		}
		simpleProtectedNegotiationTokenResponse.SupportedMechanism = mechanismIdentifier;
		simpleProtectedNegotiationTokenResponse.ResponseToken = mechanismOutput;
		return simpleProtectedNegotiationTokenResponse.GetBytes();
	}
}
