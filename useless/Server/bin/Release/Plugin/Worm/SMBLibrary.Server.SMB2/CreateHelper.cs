using System.IO;
using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class CreateHelper
{
	internal static SMB2Command GetCreateResponse(CreateRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		string text = request.Name;
		if (!text.StartsWith("\\"))
		{
			text = "\\" + text;
		}
		FileAccess requestedAccess = NTFileStoreHelper.ToCreateFileAccess(request.DesiredAccess, request.CreateDisposition);
		if (share is FileSystemShare && !((FileSystemShare)share).HasAccess(session.SecurityContext, text, requestedAccess))
		{
			state.LogToServer(Severity.Verbose, "Create: Opening '{0}{1}' failed. User '{2}' was denied access.", share.Name, text, session.UserName);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
		}
		AccessMask desiredAccess = request.DesiredAccess | (AccessMask)128u;
		object handle;
		FileStatus fileStatus;
		NTStatus nTStatus = share.FileStore.CreateFile(out handle, out fileStatus, text, desiredAccess, request.FileAttributes, request.ShareAccess, request.CreateDisposition, request.CreateOptions, session.SecurityContext);
		if (nTStatus != 0)
		{
			state.LogToServer(Severity.Verbose, "Create: Opening '{0}{1}' failed. NTStatus: {2}.", share.Name, text, nTStatus);
			return new ErrorResponse(request.CommandName, nTStatus);
		}
		FileAccess fileAccess = NTFileStoreHelper.ToFileAccess(desiredAccess);
		FileID? fileID = session.AddOpenFile(request.Header.TreeID, share.Name, text, handle, fileAccess);
		if (!fileID.HasValue)
		{
			share.FileStore.CloseFile(handle);
			state.LogToServer(Severity.Verbose, "Create: Opening '{0}{1}' failed. Too many open files.", share.Name, text);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_TOO_MANY_OPENED_FILES);
		}
		string text2 = fileAccess.ToString().Replace(", ", "|");
		string text3 = request.ShareAccess.ToString().Replace(", ", "|");
		state.LogToServer(Severity.Verbose, "Create: Opened '{0}{1}', FileAccess: {2}, ShareAccess: {3}. (SessionID: {4}, TreeID: {5}, FileId: {6})", share.Name, text, text2, text3, request.Header.SessionID, request.Header.TreeID, fileID.Value.Volatile);
		if (share is NamedPipeShare)
		{
			return CreateResponseForNamedPipe(fileID.Value, FileStatus.FILE_OPENED);
		}
		return CreateResponseFromFileSystemEntry(NTFileStoreHelper.GetNetworkOpenInformation(share.FileStore, handle), fileID.Value, fileStatus);
	}

	private static CreateResponse CreateResponseForNamedPipe(FileID fileID, FileStatus fileStatus)
	{
		return new CreateResponse
		{
			CreateAction = (CreateAction)fileStatus,
			FileAttributes = FileAttributes.Normal,
			FileId = fileID
		};
	}

	private static CreateResponse CreateResponseFromFileSystemEntry(FileNetworkOpenInformation fileInfo, FileID fileID, FileStatus fileStatus)
	{
		return new CreateResponse
		{
			CreateAction = (CreateAction)fileStatus,
			CreationTime = fileInfo.CreationTime,
			LastWriteTime = fileInfo.LastWriteTime,
			ChangeTime = fileInfo.LastWriteTime,
			LastAccessTime = fileInfo.LastAccessTime,
			AllocationSize = fileInfo.AllocationSize,
			EndofFile = fileInfo.EndOfFile,
			FileAttributes = fileInfo.FileAttributes,
			FileId = fileID
		};
	}
}
