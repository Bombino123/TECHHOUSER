using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class ChangeNotifyHelper
{
	internal static SMB2Command GetChangeNotifyInterimResponse(ChangeNotifyRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		OpenFileObject openFileObject = state.GetSession(request.Header.SessionID).GetOpenFileObject(request.FileId);
		bool watchTree = (int)(request.Flags & ChangeNotifyFlags.WatchTree) > 0;
		SMB2AsyncContext sMB2AsyncContext = state.CreateAsyncContext(request.FileId, state, request.Header.SessionID, request.Header.TreeID);
		lock (sMB2AsyncContext)
		{
			NTStatus nTStatus = share.FileStore.NotifyChange(out sMB2AsyncContext.IORequest, openFileObject.Handle, request.CompletionFilter, watchTree, (int)request.OutputBufferLength, OnNotifyChangeCompleted, sMB2AsyncContext);
			switch (nTStatus)
			{
			case NTStatus.STATUS_PENDING:
				state.LogToServer(Severity.Verbose, "NotifyChange: Monitoring of '{0}{1}' started. AsyncID: {2}.", share.Name, openFileObject.Path, sMB2AsyncContext.AsyncID);
				break;
			case NTStatus.STATUS_NOT_SUPPORTED:
				nTStatus = NTStatus.STATUS_PENDING;
				break;
			default:
				state.RemoveAsyncContext(sMB2AsyncContext);
				break;
			}
			ErrorResponse errorResponse = new ErrorResponse(request.CommandName, nTStatus);
			if (nTStatus == NTStatus.STATUS_PENDING)
			{
				errorResponse.Header.IsAsync = true;
				errorResponse.Header.AsyncID = sMB2AsyncContext.AsyncID;
			}
			return errorResponse;
		}
	}

	private static void OnNotifyChangeCompleted(NTStatus status, byte[] buffer, object context)
	{
		SMB2AsyncContext sMB2AsyncContext = (SMB2AsyncContext)context;
		lock (sMB2AsyncContext)
		{
			SMB2ConnectionState connection = sMB2AsyncContext.Connection;
			connection.RemoveAsyncContext(sMB2AsyncContext);
			SMB2Session session = connection.GetSession(sMB2AsyncContext.SessionID);
			if (session != null)
			{
				OpenFileObject openFileObject = session.GetOpenFileObject(sMB2AsyncContext.FileID);
				if (openFileObject != null)
				{
					connection.LogToServer(Severity.Verbose, "NotifyChange: Monitoring of '{0}{1}' completed. NTStatus: {2}. AsyncID: {3}", openFileObject.ShareName, openFileObject.Path, status, sMB2AsyncContext.AsyncID);
				}
				if (status == NTStatus.STATUS_SUCCESS || status == NTStatus.STATUS_NOTIFY_CLEANUP || status == NTStatus.STATUS_NOTIFY_ENUM_DIR)
				{
					ChangeNotifyResponse changeNotifyResponse = new ChangeNotifyResponse();
					changeNotifyResponse.Header.Status = status;
					changeNotifyResponse.Header.IsAsync = true;
					changeNotifyResponse.Header.IsSigned = session.SigningRequired;
					changeNotifyResponse.Header.AsyncID = sMB2AsyncContext.AsyncID;
					changeNotifyResponse.Header.SessionID = sMB2AsyncContext.SessionID;
					changeNotifyResponse.OutputBuffer = buffer;
					SMBServer.EnqueueResponse(connection, changeNotifyResponse);
				}
				else
				{
					ErrorResponse errorResponse = new ErrorResponse(SMB2CommandName.ChangeNotify);
					errorResponse.Header.Status = status;
					errorResponse.Header.IsAsync = true;
					errorResponse.Header.IsSigned = session.SigningRequired;
					errorResponse.Header.AsyncID = sMB2AsyncContext.AsyncID;
					SMBServer.EnqueueResponse(connection, errorResponse);
				}
			}
		}
	}
}
