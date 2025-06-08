using System;
using SMBLibrary.Authentication.GSSAPI;
using SMBLibrary.Authentication.NTLM;
using SMBLibrary.SMB1;

namespace SMBLibrary.Server.SMB1;

internal class NegotiateHelper
{
	public const ushort ServerMaxMpxCount = 50;

	public const ushort ServerNumberVcs = 1;

	public const ushort ServerMaxBufferSize = ushort.MaxValue;

	public const uint ServerMaxRawSize = 65536u;

	internal static NegotiateResponse GetNegotiateResponse(SMB1Header header, NegotiateRequest request, GSSProvider securityProvider, ConnectionState state)
	{
		NegotiateResponse negotiateResponse = new NegotiateResponse();
		negotiateResponse.DialectIndex = (ushort)request.Dialects.IndexOf("NT LM 0.12");
		negotiateResponse.SecurityMode = SecurityMode.UserSecurityMode | SecurityMode.EncryptPasswords;
		negotiateResponse.MaxMpxCount = 50;
		negotiateResponse.MaxNumberVcs = 1;
		negotiateResponse.MaxBufferSize = 65535u;
		negotiateResponse.MaxRawSize = 65536u;
		negotiateResponse.Capabilities = Capabilities.Unicode | Capabilities.LargeFiles | Capabilities.NTSMB | Capabilities.RpcRemoteApi | Capabilities.NTStatusCode | Capabilities.NTFind | Capabilities.InfoLevelPassthrough | Capabilities.LargeRead | Capabilities.LargeWrite;
		negotiateResponse.SystemTime = DateTime.UtcNow;
		negotiateResponse.ServerTimeZone = (short)(0.0 - TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes);
		NegotiateMessage negotiateMessage = CreateNegotiateMessage();
		if (securityProvider.GetNTLMChallengeMessage(out state.AuthenticationContext, negotiateMessage, out var challengeMessage) == NTStatus.SEC_I_CONTINUE_NEEDED)
		{
			negotiateResponse.Challenge = challengeMessage.ServerChallenge;
		}
		negotiateResponse.DomainName = string.Empty;
		negotiateResponse.ServerName = string.Empty;
		return negotiateResponse;
	}

	internal static NegotiateResponseExtended GetNegotiateResponseExtended(NegotiateRequest request, Guid serverGuid)
	{
		return new NegotiateResponseExtended
		{
			DialectIndex = (ushort)request.Dialects.IndexOf("NT LM 0.12"),
			SecurityMode = (SecurityMode.UserSecurityMode | SecurityMode.EncryptPasswords),
			MaxMpxCount = 50,
			MaxNumberVcs = 1,
			MaxBufferSize = 65535u,
			MaxRawSize = 65536u,
			Capabilities = (Capabilities.Unicode | Capabilities.LargeFiles | Capabilities.NTSMB | Capabilities.RpcRemoteApi | Capabilities.NTStatusCode | Capabilities.NTFind | Capabilities.InfoLevelPassthrough | Capabilities.LargeRead | Capabilities.LargeWrite | Capabilities.ExtendedSecurity),
			SystemTime = DateTime.UtcNow,
			ServerTimeZone = (short)(0.0 - TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes),
			ServerGuid = serverGuid
		};
	}

	private static NegotiateMessage CreateNegotiateMessage()
	{
		return new NegotiateMessage
		{
			NegotiateFlags = (NegotiateFlags.UnicodeEncoding | NegotiateFlags.OEMEncoding | NegotiateFlags.Sign | NegotiateFlags.LanManagerSessionKey | NegotiateFlags.NTLMSessionSecurity | NegotiateFlags.AlwaysSign | NegotiateFlags.Version | NegotiateFlags.Use128BitEncryption | NegotiateFlags.Use56BitEncryption),
			Version = NTLMVersion.Server2003
		};
	}
}
