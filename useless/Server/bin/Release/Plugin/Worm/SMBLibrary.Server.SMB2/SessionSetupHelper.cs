using SMBLibrary.Authentication.GSSAPI;
using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class SessionSetupHelper
{
	internal static SMB2Command GetSessionSetupResponse(SessionSetupRequest request, GSSProvider securityProvider, SMB2ConnectionState state)
	{
		SessionSetupResponse sessionSetupResponse = new SessionSetupResponse();
		byte[] outputToken;
		NTStatus nTStatus = securityProvider.AcceptSecurityContext(ref state.AuthenticationContext, request.SecurityBuffer, out outputToken);
		if (nTStatus != 0 && nTStatus != NTStatus.SEC_I_CONTINUE_NEEDED)
		{
			string text = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.UserName) as string;
			string text2 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.DomainName) as string;
			string text3 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.MachineName) as string;
			string text4 = securityProvider.GetContextAttribute(state.AuthenticationContext, GSSAttributeName.OSVersion) as string;
			state.LogToServer(Severity.Information, "Session Setup: User '{0}' failed authentication (Domain: '{1}', Workstation: '{2}', OS version: '{3}'), NTStatus: {4}", text, text2, text3, text4, nTStatus);
			return new ErrorResponse(request.CommandName, nTStatus);
		}
		if (outputToken != null)
		{
			sessionSetupResponse.SecurityBuffer = outputToken;
		}
		ulong num = request.Header.SessionID;
		if (num == 0L)
		{
			ulong? num2 = state.AllocateSessionID();
			if (!num2.HasValue)
			{
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_TOO_MANY_SESSIONS);
			}
			num = num2.Value;
			sessionSetupResponse.Header.SessionID = num2.Value;
		}
		else if (state.GetSession(num) != null)
		{
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_REQUEST_NOT_ACCEPTED);
		}
		if (nTStatus == NTStatus.SEC_I_CONTINUE_NEEDED)
		{
			sessionSetupResponse.Header.Status = NTStatus.STATUS_MORE_PROCESSING_REQUIRED;
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
				bool signingRequired = (int)(request.SecurityMode & SecurityMode.SigningRequired) > 0;
				SMB2Dialect dialect = SMBServer.ToSMB2Dialect(state.Dialect);
				byte[] signingKey = SMB2Cryptography.GenerateSigningKey(array, dialect, null);
				state.CreateSession(num, text5, text7, array, contextAttribute, signingRequired, signingKey);
			}
			else
			{
				state.LogToServer(Severity.Information, "Session Setup: User '{0}' failed authentication (Domain: '{1}', Workstation: '{2}', OS version: '{3}'), logged in as guest.", text5, text6, text7, text8);
				state.CreateSession(num, "Guest", text7, array, contextAttribute, signingRequired: false, null);
				sessionSetupResponse.SessionFlags = SessionFlags.IsGuest;
			}
		}
		return sessionSetupResponse;
	}
}
