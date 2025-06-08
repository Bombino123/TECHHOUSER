using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class CancelHelper
{
	internal static void ProcessNTCancelRequest(SMB1Header header, NTCancelRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		SMB1AsyncContext asyncContext = state.GetAsyncContext(header.UID, header.TID, header.PID, header.MID);
		if (asyncContext != null)
		{
			NTStatus nTStatus = share.FileStore.Cancel(asyncContext.IORequest);
			OpenFileObject openFileObject = session.GetOpenFileObject(asyncContext.FileID);
			if (openFileObject != null)
			{
				state.LogToServer(Severity.Information, "Cancel: Requested cancel on '{0}{1}', NTStatus: {2}. PID: {3}. MID: {4}.", share.Name, openFileObject.Path, nTStatus, asyncContext.PID, asyncContext.MID);
			}
			if (nTStatus == NTStatus.STATUS_SUCCESS || nTStatus == NTStatus.STATUS_CANCELLED)
			{
				state.RemoveAsyncContext(asyncContext);
			}
		}
	}
}
