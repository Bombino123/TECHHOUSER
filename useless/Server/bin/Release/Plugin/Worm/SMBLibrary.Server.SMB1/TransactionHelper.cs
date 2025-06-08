using System;
using System.Collections.Generic;
using System.IO;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class TransactionHelper
{
	internal static List<SMB1Command> GetTransactionResponse(SMB1Header header, TransactionRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		if (request.TransParameters.Length < request.TotalParameterCount || request.TransData.Length < request.TotalDataCount)
		{
			ProcessStateObject processStateObject = state.CreateProcessState(header.PID);
			processStateObject.MaxParameterCount = request.MaxParameterCount;
			processStateObject.MaxDataCount = request.MaxDataCount;
			processStateObject.Timeout = request.Timeout;
			processStateObject.Name = request.Name;
			processStateObject.TransactionSetup = request.Setup;
			processStateObject.TransactionParameters = new byte[request.TotalParameterCount];
			processStateObject.TransactionData = new byte[request.TotalDataCount];
			ByteWriter.WriteBytes(processStateObject.TransactionParameters, 0, request.TransParameters);
			ByteWriter.WriteBytes(processStateObject.TransactionData, 0, request.TransData);
			processStateObject.TransactionParametersReceived += request.TransParameters.Length;
			processStateObject.TransactionDataReceived += request.TransData.Length;
			if (request is Transaction2Request)
			{
				return new Transaction2InterimResponse();
			}
			return new TransactionInterimResponse();
		}
		if (request is Transaction2Request)
		{
			return GetCompleteTransaction2Response(header, request.MaxDataCount, request.Setup, request.TransParameters, request.TransData, share, state);
		}
		return GetCompleteTransactionResponse(header, request.MaxDataCount, request.Timeout, request.Name, request.Setup, request.TransParameters, request.TransData, share, state);
	}

	internal static List<SMB1Command> GetTransactionResponse(SMB1Header header, TransactionSecondaryRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		ProcessStateObject processState = state.GetProcessState(header.PID);
		if (processState == null)
		{
			throw new InvalidDataException();
		}
		ByteWriter.WriteBytes(processState.TransactionParameters, request.ParameterDisplacement, request.TransParameters);
		ByteWriter.WriteBytes(processState.TransactionData, request.DataDisplacement, request.TransData);
		processState.TransactionParametersReceived += request.TransParameters.Length;
		processState.TransactionDataReceived += request.TransData.Length;
		if (processState.TransactionParametersReceived < processState.TransactionParameters.Length || processState.TransactionDataReceived < processState.TransactionData.Length)
		{
			return new List<SMB1Command>();
		}
		state.RemoveProcessState(header.PID);
		if (request is Transaction2SecondaryRequest)
		{
			return GetCompleteTransaction2Response(header, processState.MaxDataCount, processState.TransactionSetup, processState.TransactionParameters, processState.TransactionData, share, state);
		}
		return GetCompleteTransactionResponse(header, processState.MaxDataCount, processState.Timeout, processState.Name, processState.TransactionSetup, processState.TransactionParameters, processState.TransactionData, share, state);
	}

	internal static List<SMB1Command> GetCompleteTransactionResponse(SMB1Header header, uint maxDataCount, uint timeout, string name, byte[] requestSetup, byte[] requestParameters, byte[] requestData, ISMBShare share, SMB1ConnectionState state)
	{
		if (string.Equals(name, "\\PIPE\\lanman", StringComparison.OrdinalIgnoreCase))
		{
			state.LogToServer(Severity.Debug, "Remote Administration Protocol requests are not implemented");
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
			return new ErrorResponse(CommandName.SMB_COM_TRANSACTION);
		}
		TransactionSubcommand subcommandRequest;
		try
		{
			subcommandRequest = TransactionSubcommand.GetSubcommandRequest(requestSetup, requestParameters, requestData, header.UnicodeFlag);
		}
		catch
		{
			header.Status = NTStatus.STATUS_INVALID_SMB;
			return new ErrorResponse(CommandName.SMB_COM_TRANSACTION);
		}
		state.LogToServer(Severity.Verbose, "Received complete SMB_COM_TRANSACTION subcommand: {0}", subcommandRequest.SubcommandName);
		TransactionSubcommand transactionSubcommand = null;
		if (subcommandRequest is TransactionSetNamedPipeStateRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is TransactionRawReadNamedPipeRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is TransactionQueryNamedPipeStateRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is TransactionQueryNamedPipeInfoRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is TransactionPeekNamedPipeRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is TransactionTransactNamedPipeRequest)
		{
			transactionSubcommand = TransactionSubcommandHelper.GetSubcommandResponse(header, maxDataCount, (TransactionTransactNamedPipeRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is TransactionRawWriteNamedPipeRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is TransactionReadNamedPipeRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is TransactionWriteNamedPipeRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is TransactionWaitNamedPipeRequest)
		{
			TransactionSubcommandHelper.ProcessSubcommand(header, timeout, name, (TransactionWaitNamedPipeRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is TransactionCallNamedPipeRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else
		{
			header.Status = NTStatus.STATUS_SMB_BAD_COMMAND;
		}
		if (header.Status != 0 && (header.Status != NTStatus.STATUS_BUFFER_OVERFLOW || transactionSubcommand == null))
		{
			return new ErrorResponse(CommandName.SMB_COM_TRANSACTION);
		}
		byte[] setup = transactionSubcommand.GetSetup();
		byte[] parameters = transactionSubcommand.GetParameters();
		byte[] data = transactionSubcommand.GetData(header.UnicodeFlag);
		return GetTransactionResponse(transaction2Response: false, setup, parameters, data, state.MaxBufferSize);
	}

	internal static List<SMB1Command> GetCompleteTransaction2Response(SMB1Header header, uint maxDataCount, byte[] requestSetup, byte[] requestParameters, byte[] requestData, ISMBShare share, SMB1ConnectionState state)
	{
		Transaction2Subcommand subcommandRequest;
		try
		{
			subcommandRequest = Transaction2Subcommand.GetSubcommandRequest(requestSetup, requestParameters, requestData, header.UnicodeFlag);
		}
		catch
		{
			header.Status = NTStatus.STATUS_INVALID_SMB;
			return new ErrorResponse(CommandName.SMB_COM_TRANSACTION2);
		}
		state.LogToServer(Severity.Verbose, "Received complete SMB_COM_TRANSACTION2 subcommand: {0}", subcommandRequest.SubcommandName);
		Transaction2Subcommand transaction2Subcommand = null;
		if (subcommandRequest is Transaction2FindFirst2Request)
		{
			transaction2Subcommand = Transaction2SubcommandHelper.GetSubcommandResponse(header, maxDataCount, (Transaction2FindFirst2Request)subcommandRequest, share, state);
		}
		else if (subcommandRequest is Transaction2FindNext2Request)
		{
			transaction2Subcommand = Transaction2SubcommandHelper.GetSubcommandResponse(header, maxDataCount, (Transaction2FindNext2Request)subcommandRequest, share, state);
		}
		else if (subcommandRequest is Transaction2QueryFSInformationRequest)
		{
			transaction2Subcommand = Transaction2SubcommandHelper.GetSubcommandResponse(header, maxDataCount, (Transaction2QueryFSInformationRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is Transaction2SetFSInformationRequest)
		{
			transaction2Subcommand = Transaction2SubcommandHelper.GetSubcommandResponse(header, (Transaction2SetFSInformationRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is Transaction2QueryPathInformationRequest)
		{
			transaction2Subcommand = Transaction2SubcommandHelper.GetSubcommandResponse(header, maxDataCount, (Transaction2QueryPathInformationRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is Transaction2SetPathInformationRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is Transaction2QueryFileInformationRequest)
		{
			transaction2Subcommand = Transaction2SubcommandHelper.GetSubcommandResponse(header, maxDataCount, (Transaction2QueryFileInformationRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is Transaction2SetFileInformationRequest)
		{
			transaction2Subcommand = Transaction2SubcommandHelper.GetSubcommandResponse(header, (Transaction2SetFileInformationRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is Transaction2CreateDirectoryRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is Transaction2GetDfsReferralRequest)
		{
			header.Status = NTStatus.STATUS_NO_SUCH_DEVICE;
		}
		else
		{
			header.Status = NTStatus.STATUS_SMB_BAD_COMMAND;
		}
		if (header.Status != 0 && (header.Status != NTStatus.STATUS_BUFFER_OVERFLOW || transaction2Subcommand == null))
		{
			return new ErrorResponse(CommandName.SMB_COM_TRANSACTION2);
		}
		byte[] setup = transaction2Subcommand.GetSetup();
		byte[] parameters = transaction2Subcommand.GetParameters(header.UnicodeFlag);
		byte[] data = transaction2Subcommand.GetData(header.UnicodeFlag);
		return GetTransactionResponse(transaction2Response: true, setup, parameters, data, state.MaxBufferSize);
	}

	internal static List<SMB1Command> GetTransactionResponse(bool transaction2Response, byte[] responseSetup, byte[] responseParameters, byte[] responseData, int maxBufferSize)
	{
		List<SMB1Command> list = new List<SMB1Command>();
		TransactionResponse transactionResponse = ((!transaction2Response) ? new TransactionResponse() : new Transaction2Response());
		list.Add(transactionResponse);
		int num = TransactionResponse.CalculateMessageSize(responseSetup.Length, responseParameters.Length, responseData.Length);
		if (num <= maxBufferSize)
		{
			transactionResponse.Setup = responseSetup;
			transactionResponse.TotalParameterCount = (ushort)responseParameters.Length;
			transactionResponse.TotalDataCount = (ushort)responseData.Length;
			transactionResponse.TransParameters = responseParameters;
			transactionResponse.TransData = responseData;
		}
		else
		{
			int num2 = maxBufferSize - (num - responseData.Length);
			byte[] array = new byte[num2];
			Array.Copy(responseData, 0, array, 0, num2);
			transactionResponse.Setup = responseSetup;
			transactionResponse.TotalParameterCount = (ushort)responseParameters.Length;
			transactionResponse.TotalDataCount = (ushort)responseData.Length;
			transactionResponse.TransParameters = responseParameters;
			transactionResponse.TransData = array;
			for (int num3 = responseData.Length - num2; num3 > 0; num3 -= num2)
			{
				TransactionResponse transactionResponse2 = ((!transaction2Response) ? new TransactionResponse() : new Transaction2Response());
				num2 = num3;
				num = TransactionResponse.CalculateMessageSize(0, 0, num3);
				if (num > maxBufferSize)
				{
					num2 = maxBufferSize - (num - num3);
				}
				array = new byte[num2];
				int num4 = responseData.Length - num3;
				Array.Copy(responseData, num4, array, 0, num2);
				transactionResponse2.TotalParameterCount = (ushort)responseParameters.Length;
				transactionResponse2.TotalDataCount = (ushort)responseData.Length;
				transactionResponse2.TransData = array;
				transactionResponse2.ParameterDisplacement = (ushort)transactionResponse.TransParameters.Length;
				transactionResponse2.DataDisplacement = (ushort)num4;
				list.Add(transactionResponse2);
			}
		}
		return list;
	}
}
