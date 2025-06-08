using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class CancelHelper
{
	internal static SMB2Command GetCancelResponse(CancelRequest request, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		if (request.Header.IsAsync)
		{
			SMB2AsyncContext asyncContext = state.GetAsyncContext(request.Header.AsyncID);
			if (asyncContext != null)
			{
				ISMBShare connectedTree = session.GetConnectedTree(asyncContext.TreeID);
				OpenFileObject openFileObject = session.GetOpenFileObject(asyncContext.FileID);
				NTStatus nTStatus = connectedTree.FileStore.Cancel(asyncContext.IORequest);
				if (openFileObject != null)
				{
					state.LogToServer(Severity.Information, "Cancel: Requested cancel on '{0}{1}'. NTStatus: {2}, AsyncID: {3}.", connectedTree.Name, openFileObject.Path, nTStatus, asyncContext.AsyncID);
				}
				if (nTStatus == NTStatus.STATUS_SUCCESS || nTStatus == NTStatus.STATUS_CANCELLED || nTStatus == NTStatus.STATUS_NOT_SUPPORTED)
				{
					state.RemoveAsyncContext(asyncContext);
					return new ErrorResponse(request.CommandName, NTStatus.STATUS_CANCELLED)
					{
						Header = 
						{
							IsAsync = true,
							AsyncID = asyncContext.AsyncID
						}
					};
				}
				return null;
			}
			return null;
		}
		return null;
	}
}
