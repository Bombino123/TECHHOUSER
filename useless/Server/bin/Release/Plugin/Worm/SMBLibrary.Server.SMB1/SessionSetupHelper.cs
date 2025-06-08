using SMBLibrary.Authentication.GSSAPI;
using SMBLibrary.Authentication.NTLM;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class SessionSetupHelper
{
	internal static SMB1Command GetSessionSetupResponse(SMB1Header header, SessionSetupAndXRequest request, GSSProvider securityProvider, SMB1ConnectionState state)
	{
		SessionSetupAndXResponse sessionSetupAndXResponse = new SessionSetupAndXResponse();
		AuthenticateMessage authenticateMessage = CreateAuthenticateMessage(request.AccountName, request.OEMPassword, request.UnicodePassword);
		header.Status = securityProvider.NTLMAuthenticate(state.AuthenticationContext, authenticateMessage);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Information, "Session Setup: User '{0}' failed authentication (Domain: '{1}', OS: '{2}'), NTStatus: {3}", request.AccountName, request.PrimaryDomain, request.NativeOS, header.Status);
			return new ErrorResponse(request.CommandName);
		}
		string text = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.OSVersion) as string;
		byte[] array = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.SessionKey) as byte[];
		object contextAttribute = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.AccessToken);
		bool? flag = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.IsGuest) as bool?;
		if (array != null && array.Length > 16)
		{
			array = ByteReader.ReadBytes(array, 0, 16);
		}
		SMB1Session sMB1Session;
		if (!flag.HasValue || !flag.Value)
		{
			state.LogToServer(Severity.Information, "Session Setup: User '{0}' authenticated successfully (Domain: '{1}', Workstation: '{2}', OS version: '{3}').", authenticateMessage.UserName, authenticateMessage.DomainName, authenticateMessage.WorkStation, text);
			sMB1Session = state.CreateSession(authenticateMessage.UserName, authenticateMessage.WorkStation, array, contextAttribute);
		}
		else
		{
			state.LogToServer(Severity.Information, "Session Setup: User '{0}' failed authentication (Domain: '{1}', Workstation: '{2}', OS version: '{3}'), logged in as guest.", authenticateMessage.UserName, authenticateMessage.DomainName, authenticateMessage.WorkStation, text);
			sMB1Session = state.CreateSession("Guest", authenticateMessage.WorkStation, array, contextAttribute);
			sessionSetupAndXResponse.Action = SessionSetupAction.SetupGuest;
		}
		if (sMB1Session == null)
		{
			header.Status = NTStatus.STATUS_TOO_MANY_SESSIONS;
			return new ErrorResponse(request.CommandName);
		}
		header.UID = sMB1Session.UserID;
		sessionSetupAndXResponse.PrimaryDomain = request.PrimaryDomain;
		if ((request.Capabilities & Capabilities.LargeRead) != 0)
		{
			state.LargeRead = true;
		}
		if ((request.Capabilities & Capabilities.LargeWrite) != 0)
		{
			state.LargeWrite = true;
		}
		sessionSetupAndXResponse.NativeOS = string.Empty;
		sessionSetupAndXResponse.NativeLanMan = string.Empty;
		return sessionSetupAndXResponse;
	}

	internal static SMB1Command GetSessionSetupResponseExtended(SMB1Header header, SessionSetupAndXRequestExtended request, GSSProvider securityProvider, SMB1ConnectionState state)
	{
		SessionSetupAndXResponseExtended sessionSetupAndXResponseExtended = new SessionSetupAndXResponseExtended();
		byte[] outputToken;
		NTStatus nTStatus = securityProvider.AcceptSecurityContext(ref state.AuthenticationContext, request.SecurityBlob, out outputToken);
		if (nTStatus != 0 && nTStatus != NTStatus.SEC_I_CONTINUE_NEEDED)
		{
			string text = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.UserName) as string;
			string text2 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.DomainName) as string;
			string text3 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.MachineName) as string;
			string text4 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.OSVersion) as string;
			state.LogToServer(Severity.Information, "Session Setup: User '{0}' failed authentication (Domain: '{1}', Workstation: '{2}', OS version: '{3}'), NTStatus: {4}", text, text2, text3, text4, nTStatus);
			header.Status = nTStatus;
			return new ErrorResponse(request.CommandName);
		}
		if (outputToken != null)
		{
			sessionSetupAndXResponseExtended.SecurityBlob = outputToken;
		}
		if (header.UID == 0)
		{
			ushort? num = state.AllocateUserID();
			if (!num.HasValue)
			{
				header.Status = NTStatus.STATUS_TOO_MANY_SESSIONS;
				return new ErrorResponse(request.CommandName);
			}
			header.UID = num.Value;
		}
		if (nTStatus == NTStatus.SEC_I_CONTINUE_NEEDED)
		{
			header.Status = NTStatus.STATUS_MORE_PROCESSING_REQUIRED;
		}
		else
		{
			string text5 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.UserName) as string;
			string text6 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.DomainName) as string;
			string text7 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.MachineName) as string;
			string text8 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.OSVersion) as string;
			byte[] array = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.SessionKey) as byte[];
			object contextAttribute = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.AccessToken);
			bool? flag = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.IsGuest) as bool?;
			if (array != null && array.Length > 16)
			{
				array = ByteReader.ReadBytes(array, 0, 16);
			}
			if (!flag.HasValue || !flag.Value)
			{
				state.LogToServer(Severity.Information, "Session Setup: User '{0}' authenticated successfully (Domain: '{1}', Workstation: '{2}', OS version: '{3}').", text5, text6, text7, text8);
				state.CreateSession(header.UID, text5, text7, array, contextAttribute);
			}
			else
			{
				state.LogToServer(Severity.Information, "Session Setup: User '{0}' failed authentication (Domain: '{1}', Workstation: '{2}', OS version: '{3}'), logged in as guest.", text5, text6, text7, text8);
				state.CreateSession(header.UID, "Guest", text7, array, contextAttribute);
				sessionSetupAndXResponseExtended.Action = SessionSetupAction.SetupGuest;
			}
		}
		sessionSetupAndXResponseExtended.NativeOS = string.Empty;
		sessionSetupAndXResponseExtended.NativeLanMan = string.Empty;
		return sessionSetupAndXResponseExtended;
	}

	private static AuthenticateMessage CreateAuthenticateMessage(string accountNameToAuth, byte[] lmChallengeResponse, byte[] ntChallengeResponse)
	{
		AuthenticateMessage authenticateMessage = new AuthenticateMessage();
		authenticateMessage.NegotiateFlags = NegotiateFlags.UnicodeEncoding | NegotiateFlags.OEMEncoding | NegotiateFlags.Sign | NegotiateFlags.NTLMSessionSecurity | NegotiateFlags.AlwaysSign | NegotiateFlags.Version | NegotiateFlags.Use128BitEncryption | NegotiateFlags.Use56BitEncryption;
		if (AuthenticationMessageUtils.IsNTLMv1ExtendedSessionSecurity(lmChallengeResponse) || AuthenticationMessageUtils.IsNTLMv2NTResponse(ntChallengeResponse))
		{
			authenticateMessage.NegotiateFlags |= NegotiateFlags.ExtendedSessionSecurity;
		}
		else
		{
			authenticateMessage.NegotiateFlags |= NegotiateFlags.LanManagerSessionKey;
		}
		authenticateMessage.UserName = accountNameToAuth;
		authenticateMessage.LmChallengeResponse = lmChallengeResponse;
		authenticateMessage.NtChallengeResponse = ntChallengeResponse;
		authenticateMessage.Version = NTLMVersion.Server2003;
		return authenticateMessage;
	}
}
