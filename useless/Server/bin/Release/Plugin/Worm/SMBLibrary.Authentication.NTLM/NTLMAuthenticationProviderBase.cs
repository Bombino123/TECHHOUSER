using System.Runtime.InteropServices;
using SMBLibrary.Authentication.GSSAPI;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public abstract class NTLMAuthenticationProviderBase : IGSSMechanism
{
	public static readonly byte[] NTLMSSPIdentifier = new byte[10] { 43, 6, 1, 4, 1, 130, 55, 2, 2, 10 };

	public byte[] Identifier => NTLMSSPIdentifier;

	public NTStatus AcceptSecurityContext(ref object context, byte[] inputToken, out byte[] outputToken)
	{
		outputToken = null;
		if (!AuthenticationMessageUtils.IsSignatureValid(inputToken))
		{
			return NTStatus.SEC_E_INVALID_TOKEN;
		}
		return AuthenticationMessageUtils.GetMessageType(inputToken) switch
		{
			MessageTypeName.Negotiate => GetChallengeMessage(out context, inputToken, out outputToken), 
			MessageTypeName.Authenticate => Authenticate(context, inputToken), 
			_ => NTStatus.SEC_E_INVALID_TOKEN, 
		};
	}

	public abstract NTStatus GetChallengeMessage(out object context, byte[] negotiateMessageBytes, out byte[] challengeMessageBytes);

	public abstract NTStatus Authenticate(object context, byte[] authenticateMessageBytes);

	public abstract bool DeleteSecurityContext(ref object context);

	public abstract object GetContextAttribute(object context, GSSAttributeName attributeName);
}
