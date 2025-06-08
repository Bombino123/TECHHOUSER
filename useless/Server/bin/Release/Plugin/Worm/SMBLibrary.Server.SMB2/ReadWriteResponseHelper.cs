using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class ReadWriteResponseHelper
{
	internal static SMB2Command GetReadResponse(ReadRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FileId);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Read failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "Read from '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
		}
		byte[] data;
		NTStatus nTStatus = share.FileStore.ReadFile(out data, openFileObject.Handle, (long)request.Offset, (int)request.ReadLength);
		if (nTStatus != 0)
		{
			state.LogToServer(Severity.Verbose, "Read from '{0}{1}' failed. NTStatus: {2}. (FileId: {3})", share.Name, openFileObject.Path, nTStatus, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, nTStatus);
		}
		return new ReadResponse
		{
			Data = data
		};
	}

	internal static SMB2Command GetWriteResponse(WriteRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FileId);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Write failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "Write to '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
		}
		int numberOfBytesWritten;
		NTStatus nTStatus = share.FileStore.WriteFile(out numberOfBytesWritten, openFileObject.Handle, (long)request.Offset, request.Data);
		if (nTStatus != 0)
		{
			state.LogToServer(Severity.Verbose, "Write to '{0}{1}' failed. NTStatus: {2}. (FileId: {3})", share.Name, openFileObject.Path, nTStatus, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, nTStatus);
		}
		return new WriteResponse
		{
			Count = (uint)numberOfBytesWritten
		};
	}

	internal static SMB2Command GetFlushResponse(FlushRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		OpenFileObject openFileObject = state.GetSession(request.Header.SessionID).GetOpenFileObject(request.FileId);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Flush failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
		}
		NTStatus nTStatus = share.FileStore.FlushFileBuffers(openFileObject.Handle);
		if (nTStatus != 0)
		{
			state.LogToServer(Severity.Verbose, "Flush '{0}{1}' failed. NTStatus: {2}. (FileId: {3})", share.Name, openFileObject.Path, nTStatus, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, nTStatus);
		}
		return new FlushResponse();
	}
}
