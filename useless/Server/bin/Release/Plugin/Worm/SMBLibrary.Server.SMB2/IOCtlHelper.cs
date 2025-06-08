using System;
using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class IOCtlHelper
{
	internal static SMB2Command GetIOCtlResponse(IOCtlRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		string text;
		if (!Enum.IsDefined(typeof(IoControlCode), request.CtlCode))
		{
			text = "0x" + request.CtlCode.ToString("X8");
		}
		else
		{
			IoControlCode ctlCode = (IoControlCode)request.CtlCode;
			text = ctlCode.ToString();
		}
		string text2 = text;
		if (!request.IsFSCtl)
		{
			state.LogToServer(Severity.Verbose, "IOCTL: Non-FSCTL requests are not supported. CTL Code: {0}", text2);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_NOT_SUPPORTED);
		}
		if (request.CtlCode == 393620 || request.CtlCode == 393648)
		{
			state.LogToServer(Severity.Verbose, "IOCTL failed. CTL Code: {0}. NTStatus: STATUS_FS_DRIVER_REQUIRED.", text2);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_FS_DRIVER_REQUIRED);
		}
		object handle;
		if (request.CtlCode == 1114136 || request.CtlCode == 1311236 || request.CtlCode == 1311228)
		{
			if (request.FileId.Persistent != ulong.MaxValue || request.FileId.Volatile != ulong.MaxValue)
			{
				state.LogToServer(Severity.Verbose, "IOCTL failed. CTL Code: {0}. FileId MUST be 0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", text2);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
			}
			handle = null;
		}
		else
		{
			OpenFileObject openFileObject = session.GetOpenFileObject(request.FileId);
			if (openFileObject == null)
			{
				state.LogToServer(Severity.Verbose, "IOCTL failed. CTL Code: {0}. Invalid FileId. (SessionID: {1}, TreeID: {2}, FileId: {3})", text2, request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
			}
			handle = openFileObject.Handle;
		}
		int maxOutputResponse = (int)request.MaxOutputResponse;
		byte[] output;
		NTStatus nTStatus = share.FileStore.DeviceIOControl(handle, request.CtlCode, request.Input, out output, maxOutputResponse);
		if (nTStatus != 0 && nTStatus != NTStatus.STATUS_BUFFER_OVERFLOW)
		{
			state.LogToServer(Severity.Verbose, "IOCTL failed. CTL Code: {0}. NTStatus: {1}.", text2, nTStatus);
			return new ErrorResponse(request.CommandName, nTStatus);
		}
		state.LogToServer(Severity.Verbose, "IOCTL succeeded. CTL Code: {0}.", text2);
		return new IOCtlResponse
		{
			Header = 
			{
				Status = nTStatus
			},
			CtlCode = request.CtlCode,
			FileId = request.FileId,
			Output = output
		};
	}
}
