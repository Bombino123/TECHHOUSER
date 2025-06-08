using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class CloseHelper
{
	internal static SMB2Command GetCloseResponse(CloseRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FileId);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Close failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
		}
		NTStatus nTStatus = share.FileStore.CloseFile(openFileObject.Handle);
		if (nTStatus != 0)
		{
			state.LogToServer(Severity.Information, "Close: Closing '{0}{1}' failed. NTStatus: {2}. (SessionID: {3}, TreeID: {4}, FileId: {5})", share.Name, openFileObject.Path, nTStatus, request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, nTStatus);
		}
		state.LogToServer(Severity.Information, "Close: Closed '{0}{1}'. (SessionID: {2}, TreeID: {3}, FileId: {4})", share.Name, openFileObject.Path, request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
		session.RemoveOpenFile(request.FileId);
		CloseResponse closeResponse = new CloseResponse();
		if (request.PostQueryAttributes)
		{
			FileNetworkOpenInformation networkOpenInformation = NTFileStoreHelper.GetNetworkOpenInformation(share.FileStore, openFileObject.Path, session.SecurityContext);
			if (networkOpenInformation != null)
			{
				closeResponse.CreationTime = networkOpenInformation.CreationTime;
				closeResponse.LastAccessTime = networkOpenInformation.LastAccessTime;
				closeResponse.LastWriteTime = networkOpenInformation.LastWriteTime;
				closeResponse.ChangeTime = networkOpenInformation.ChangeTime;
				closeResponse.AllocationSize = networkOpenInformation.AllocationSize;
				closeResponse.EndofFile = networkOpenInformation.EndOfFile;
				closeResponse.FileAttributes = networkOpenInformation.FileAttributes;
			}
		}
		return closeResponse;
	}
}
