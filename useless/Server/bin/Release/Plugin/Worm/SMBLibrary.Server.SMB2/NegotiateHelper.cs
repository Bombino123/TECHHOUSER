using System;
using System.Collections.Generic;
using SMBLibrary.Authentication.GSSAPI;
using SMBLibrary.SMB1;
using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class NegotiateHelper
{
	public const string SMB2002Dialect = "SMB 2.002";

	public const string SMB2xxxDialect = "SMB 2.???";

	public const uint ServerMaxTransactSize = 65536u;

	public const uint ServerMaxReadSize = 65536u;

	public const uint ServerMaxWriteSize = 65536u;

	public const uint ServerMaxTransactSizeLargeMTU = 1048576u;

	public const uint ServerMaxReadSizeLargeMTU = 1048576u;

	public const uint ServerMaxWriteSizeLargeMTU = 1048576u;

	internal static SMB2Command GetNegotiateResponse(List<string> smb2Dialects, GSSProvider securityProvider, ConnectionState state, SMBTransportType transportType, Guid serverGuid, DateTime serverStartTime)
	{
		SMBLibrary.SMB2.NegotiateResponse negotiateResponse = new SMBLibrary.SMB2.NegotiateResponse();
		negotiateResponse.Header.Credits = 1;
		if (smb2Dialects.Contains("SMB 2.???"))
		{
			negotiateResponse.DialectRevision = SMB2Dialect.SMB2xx;
		}
		else
		{
			if (!smb2Dialects.Contains("SMB 2.002"))
			{
				throw new ArgumentException("SMB2 dialect is not present");
			}
			state.Dialect = SMBDialect.SMB202;
			negotiateResponse.DialectRevision = SMB2Dialect.SMB202;
		}
		negotiateResponse.SecurityMode = SMBLibrary.SMB2.SecurityMode.SigningEnabled;
		negotiateResponse.ServerGuid = serverGuid;
		if (state.Dialect != SMBDialect.SMB202 && transportType == SMBTransportType.DirectTCPTransport)
		{
			negotiateResponse.Capabilities = SMBLibrary.SMB2.Capabilities.LargeMTU;
			negotiateResponse.MaxTransactSize = 1048576u;
			negotiateResponse.MaxReadSize = 1048576u;
			negotiateResponse.MaxWriteSize = 1048576u;
			int num = 1048836;
			if (num > state.ReceiveBuffer.Buffer.Length)
			{
				state.ReceiveBuffer.IncreaseBufferSize(num);
			}
		}
		else
		{
			negotiateResponse.MaxTransactSize = 65536u;
			negotiateResponse.MaxReadSize = 65536u;
			negotiateResponse.MaxWriteSize = 65536u;
		}
		negotiateResponse.SystemTime = DateTime.Now;
		negotiateResponse.ServerStartTime = serverStartTime;
		negotiateResponse.SecurityBuffer = securityProvider.GetSPNEGOTokenInitBytes();
		return negotiateResponse;
	}

	internal static SMB2Command GetNegotiateResponse(SMBLibrary.SMB2.NegotiateRequest request, GSSProvider securityProvider, ConnectionState state, SMBTransportType transportType, Guid serverGuid, DateTime serverStartTime, bool enableSMB3)
	{
		SMBLibrary.SMB2.NegotiateResponse negotiateResponse = new SMBLibrary.SMB2.NegotiateResponse();
		if (enableSMB3 && request.Dialects.Contains(SMB2Dialect.SMB300))
		{
			state.Dialect = SMBDialect.SMB300;
			negotiateResponse.DialectRevision = SMB2Dialect.SMB300;
		}
		else if (request.Dialects.Contains(SMB2Dialect.SMB210))
		{
			state.Dialect = SMBDialect.SMB210;
			negotiateResponse.DialectRevision = SMB2Dialect.SMB210;
		}
		else
		{
			if (!request.Dialects.Contains(SMB2Dialect.SMB202))
			{
				state.LogToServer(Severity.Verbose, "Negotiate failure: None of the requested SMB2 dialects is supported");
				return new SMBLibrary.SMB2.ErrorResponse(request.CommandName, NTStatus.STATUS_NOT_SUPPORTED);
			}
			state.Dialect = SMBDialect.SMB202;
			negotiateResponse.DialectRevision = SMB2Dialect.SMB202;
		}
		negotiateResponse.SecurityMode = SMBLibrary.SMB2.SecurityMode.SigningEnabled;
		negotiateResponse.ServerGuid = serverGuid;
		if (state.Dialect != SMBDialect.SMB202 && transportType == SMBTransportType.DirectTCPTransport)
		{
			negotiateResponse.Capabilities = SMBLibrary.SMB2.Capabilities.LargeMTU;
			negotiateResponse.MaxTransactSize = 1048576u;
			negotiateResponse.MaxReadSize = 1048576u;
			negotiateResponse.MaxWriteSize = 1048576u;
			int num = 1048836;
			if (num > state.ReceiveBuffer.Buffer.Length)
			{
				state.ReceiveBuffer.IncreaseBufferSize(num);
			}
		}
		else
		{
			negotiateResponse.MaxTransactSize = 65536u;
			negotiateResponse.MaxReadSize = 65536u;
			negotiateResponse.MaxWriteSize = 65536u;
		}
		negotiateResponse.SystemTime = DateTime.Now;
		negotiateResponse.ServerStartTime = serverStartTime;
		negotiateResponse.SecurityBuffer = securityProvider.GetSPNEGOTokenInitBytes();
		return negotiateResponse;
	}

	internal static List<string> FindSMB2Dialects(SMB1Message message)
	{
		if (message.Commands.Count > 0 && message.Commands[0] is SMBLibrary.SMB1.NegotiateRequest)
		{
			return FindSMB2Dialects((SMBLibrary.SMB1.NegotiateRequest)message.Commands[0]);
		}
		return new List<string>();
	}

	internal static List<string> FindSMB2Dialects(SMBLibrary.SMB1.NegotiateRequest request)
	{
		List<string> list = new List<string>();
		if (request.Dialects.Contains("SMB 2.002"))
		{
			list.Add("SMB 2.002");
		}
		if (request.Dialects.Contains("SMB 2.???"))
		{
			list.Add("SMB 2.???");
		}
		return list;
	}
}
