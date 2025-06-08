using System;
using System.Collections.Generic;
using System.IO;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class NTTransactHelper
{
	internal static List<SMB1Command> GetNTTransactResponse(SMB1Header header, NTTransactRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		if (request.TransParameters.Length < request.TotalParameterCount || request.TransData.Length < request.TotalDataCount)
		{
			ProcessStateObject processStateObject = state.CreateProcessState(header.PID);
			processStateObject.SubcommandID = (ushort)request.Function;
			processStateObject.MaxParameterCount = request.MaxParameterCount;
			processStateObject.MaxDataCount = request.MaxDataCount;
			processStateObject.TransactionSetup = request.Setup;
			processStateObject.TransactionParameters = new byte[request.TotalParameterCount];
			processStateObject.TransactionData = new byte[request.TotalDataCount];
			ByteWriter.WriteBytes(processStateObject.TransactionParameters, 0, request.TransParameters);
			ByteWriter.WriteBytes(processStateObject.TransactionData, 0, request.TransData);
			processStateObject.TransactionParametersReceived += request.TransParameters.Length;
			processStateObject.TransactionDataReceived += request.TransData.Length;
			return new NTTransactInterimResponse();
		}
		return GetCompleteNTTransactResponse(header, request.MaxParameterCount, request.MaxDataCount, request.Function, request.Setup, request.TransParameters, request.TransData, share, state);
	}

	internal static List<SMB1Command> GetNTTransactResponse(SMB1Header header, NTTransactSecondaryRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		ProcessStateObject processState = state.GetProcessState(header.PID);
		if (processState == null)
		{
			throw new InvalidDataException();
		}
		ByteWriter.WriteBytes(processState.TransactionParameters, (int)request.ParameterDisplacement, request.TransParameters);
		ByteWriter.WriteBytes(processState.TransactionData, (int)request.DataDisplacement, request.TransData);
		processState.TransactionParametersReceived += request.TransParameters.Length;
		processState.TransactionDataReceived += request.TransData.Length;
		if (processState.TransactionParametersReceived < processState.TransactionParameters.Length || processState.TransactionDataReceived < processState.TransactionData.Length)
		{
			return new List<SMB1Command>();
		}
		state.RemoveProcessState(header.PID);
		return GetCompleteNTTransactResponse(header, processState.MaxParameterCount, processState.MaxDataCount, (NTTransactSubcommandName)processState.SubcommandID, processState.TransactionSetup, processState.TransactionParameters, processState.TransactionData, share, state);
	}

	internal static List<SMB1Command> GetCompleteNTTransactResponse(SMB1Header header, uint maxParameterCount, uint maxDataCount, NTTransactSubcommandName subcommandName, byte[] requestSetup, byte[] requestParameters, byte[] requestData, ISMBShare share, SMB1ConnectionState state)
	{
		NTTransactSubcommand subcommandRequest;
		try
		{
			subcommandRequest = NTTransactSubcommand.GetSubcommandRequest(subcommandName, requestSetup, requestParameters, requestData, header.UnicodeFlag);
		}
		catch
		{
			header.Status = NTStatus.STATUS_INVALID_SMB;
			return new ErrorResponse(CommandName.SMB_COM_NT_TRANSACT);
		}
		state.LogToServer(Severity.Verbose, "Received complete SMB_COM_NT_TRANSACT subcommand: {0}", subcommandRequest.SubcommandName);
		NTTransactSubcommand nTTransactSubcommand = null;
		if (subcommandRequest is NTTransactCreateRequest)
		{
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
		}
		else if (subcommandRequest is NTTransactIOCTLRequest)
		{
			nTTransactSubcommand = GetSubcommandResponse(header, maxDataCount, (NTTransactIOCTLRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is NTTransactSetSecurityDescriptorRequest)
		{
			nTTransactSubcommand = GetSubcommandResponse(header, (NTTransactSetSecurityDescriptorRequest)subcommandRequest, share, state);
		}
		else if (subcommandRequest is NTTransactNotifyChangeRequest)
		{
			NotifyChangeHelper.ProcessNTTransactNotifyChangeRequest(header, maxParameterCount, (NTTransactNotifyChangeRequest)subcommandRequest, share, state);
			if (header.Status == NTStatus.STATUS_PENDING)
			{
				return new List<SMB1Command>();
			}
		}
		else if (subcommandRequest is NTTransactQuerySecurityDescriptorRequest)
		{
			nTTransactSubcommand = GetSubcommandResponse(header, maxDataCount, (NTTransactQuerySecurityDescriptorRequest)subcommandRequest, share, state);
		}
		else
		{
			header.Status = NTStatus.STATUS_SMB_BAD_COMMAND;
		}
		if (header.Status != 0 && (header.Status != NTStatus.STATUS_BUFFER_OVERFLOW || nTTransactSubcommand == null))
		{
			return new ErrorResponse(CommandName.SMB_COM_NT_TRANSACT);
		}
		byte[] setup = nTTransactSubcommand.GetSetup();
		byte[] parameters = nTTransactSubcommand.GetParameters(header.UnicodeFlag);
		byte[] data = nTTransactSubcommand.GetData();
		return GetNTTransactResponse(setup, parameters, data, state.MaxBufferSize);
	}

	private static NTTransactIOCTLResponse GetSubcommandResponse(SMB1Header header, uint maxDataCount, NTTransactIOCTLRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		string text;
		if (!Enum.IsDefined(typeof(IoControlCode), subcommand.FunctionCode))
		{
			text = "0x" + subcommand.FunctionCode.ToString("X8");
		}
		else
		{
			IoControlCode functionCode = (IoControlCode)subcommand.FunctionCode;
			text = functionCode.ToString();
		}
		string text2 = text;
		if (!subcommand.IsFsctl)
		{
			state.LogToServer(Severity.Verbose, "IOCTL: Non-FSCTL requests are not supported. CTL Code: {0}", text2);
			header.Status = NTStatus.STATUS_NOT_SUPPORTED;
			return null;
		}
		OpenFileObject openFileObject = session.GetOpenFileObject(subcommand.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "IOCTL failed. CTL Code: {0}. Invalid FID. (UID: {1}, TID: {2}, FID: {3})", text2, header.UID, header.TID, subcommand.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return null;
		}
		header.Status = share.FileStore.DeviceIOControl(openFileObject.Handle, subcommand.FunctionCode, subcommand.Data, out var output, (int)maxDataCount);
		if (header.Status != 0 && header.Status != NTStatus.STATUS_BUFFER_OVERFLOW)
		{
			state.LogToServer(Severity.Verbose, "IOCTL failed. CTL Code: {0}. NTStatus: {1}. (FID: {2})", text2, header.Status, subcommand.FID);
			return null;
		}
		state.LogToServer(Severity.Verbose, "IOCTL succeeded. CTL Code: {0}. (FID: {1})", text2, subcommand.FID);
		return new NTTransactIOCTLResponse
		{
			Data = output
		};
	}

	private static NTTransactSetSecurityDescriptorResponse GetSubcommandResponse(SMB1Header header, NTTransactSetSecurityDescriptorRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		OpenFileObject openFileObject = state.GetSession(header.UID).GetOpenFileObject(subcommand.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "SetSecurityInformation failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, subcommand.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return null;
		}
		header.Status = share.FileStore.SetSecurityInformation(openFileObject.Handle, subcommand.SecurityInformation, subcommand.SecurityDescriptor);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "SetSecurityInformation on '{0}{1}' failed. Security information: 0x{2}, NTStatus: {3}. (FID: {4})", share.Name, openFileObject.Path, subcommand.SecurityInformation.ToString("X"), header.Status, subcommand.FID);
			return null;
		}
		state.LogToServer(Severity.Verbose, "SetSecurityInformation on '{0}{1}' succeeded. Security information: 0x{2}. (FID: {3})", share.Name, openFileObject.Path, subcommand.SecurityInformation.ToString("X"), subcommand.FID);
		return new NTTransactSetSecurityDescriptorResponse();
	}

	private static NTTransactQuerySecurityDescriptorResponse GetSubcommandResponse(SMB1Header header, uint maxDataCount, NTTransactQuerySecurityDescriptorRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		OpenFileObject openFileObject = state.GetSession(header.UID).GetOpenFileObject(subcommand.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "GetSecurityInformation failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, subcommand.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return null;
		}
		header.Status = share.FileStore.GetSecurityInformation(out var result, openFileObject.Handle, subcommand.SecurityInfoFields);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "GetSecurityInformation on '{0}{1}' failed. Security information: 0x{2}, NTStatus: {3}. (FID: {4})", share.Name, openFileObject.Path, subcommand.SecurityInfoFields.ToString("X"), header.Status, subcommand.FID);
			return null;
		}
		NTTransactQuerySecurityDescriptorResponse nTTransactQuerySecurityDescriptorResponse = new NTTransactQuerySecurityDescriptorResponse();
		nTTransactQuerySecurityDescriptorResponse.LengthNeeded = (uint)result.Length;
		if (nTTransactQuerySecurityDescriptorResponse.LengthNeeded <= maxDataCount)
		{
			state.LogToServer(Severity.Verbose, "GetSecurityInformation on '{0}{1}' succeeded. Security information: 0x{2}. (FID: {3})", share.Name, openFileObject.Path, subcommand.SecurityInfoFields.ToString("X"), subcommand.FID);
			nTTransactQuerySecurityDescriptorResponse.SecurityDescriptor = result;
		}
		else
		{
			state.LogToServer(Severity.Verbose, "GetSecurityInformation on '{0}{1}' failed. Security information: 0x{2}, NTStatus: STATUS_BUFFER_TOO_SMALL. (FID: {3})", share.Name, openFileObject.Path, subcommand.SecurityInfoFields.ToString("X"), subcommand.FID);
			header.Status = NTStatus.STATUS_BUFFER_TOO_SMALL;
		}
		return nTTransactQuerySecurityDescriptorResponse;
	}

	internal static List<SMB1Command> GetNTTransactResponse(byte[] responseSetup, byte[] responseParameters, byte[] responseData, int maxBufferSize)
	{
		List<SMB1Command> list = new List<SMB1Command>();
		NTTransactResponse nTTransactResponse = new NTTransactResponse();
		list.Add(nTTransactResponse);
		int num = NTTransactResponse.CalculateMessageSize(responseSetup.Length, responseParameters.Length, responseData.Length);
		if (num <= maxBufferSize)
		{
			nTTransactResponse.Setup = responseSetup;
			nTTransactResponse.TotalParameterCount = (ushort)responseParameters.Length;
			nTTransactResponse.TotalDataCount = (ushort)responseData.Length;
			nTTransactResponse.TransParameters = responseParameters;
			nTTransactResponse.TransData = responseData;
		}
		else
		{
			int num2 = maxBufferSize - (num - responseData.Length);
			byte[] array = new byte[num2];
			Array.Copy(responseData, 0, array, 0, num2);
			nTTransactResponse.Setup = responseSetup;
			nTTransactResponse.TotalParameterCount = (ushort)responseParameters.Length;
			nTTransactResponse.TotalDataCount = (ushort)responseData.Length;
			nTTransactResponse.TransParameters = responseParameters;
			nTTransactResponse.TransData = array;
			for (int num3 = responseData.Length - num2; num3 > 0; num3 -= num2)
			{
				NTTransactResponse nTTransactResponse2 = new NTTransactResponse();
				num2 = num3;
				num = TransactionResponse.CalculateMessageSize(0, 0, num3);
				if (num > maxBufferSize)
				{
					num2 = maxBufferSize - (num - num3);
				}
				array = new byte[num2];
				int num4 = responseData.Length - num3;
				Array.Copy(responseData, num4, array, 0, num2);
				nTTransactResponse2.TotalParameterCount = (ushort)responseParameters.Length;
				nTTransactResponse2.TotalDataCount = (ushort)responseData.Length;
				nTTransactResponse2.TransData = array;
				nTTransactResponse2.ParameterDisplacement = (ushort)nTTransactResponse.TransParameters.Length;
				nTTransactResponse2.DataDisplacement = (ushort)num4;
				list.Add(nTTransactResponse2);
			}
		}
		return list;
	}
}
