using System;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class FileStoreResponseHelper
{
	internal static SMB1Command GetCreateDirectoryResponse(SMB1Header header, CreateDirectoryRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, request.DirectoryName))
		{
			state.LogToServer(Severity.Verbose, "Create Directory '{0}{1}' failed. User '{2}' was denied access.", share.Name, request.DirectoryName, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = SMB1FileStoreHelper.CreateDirectory(share.FileStore, request.DirectoryName, session.SecurityContext);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Create Directory '{0}{1}' failed. NTStatus: {2}.", share.Name, request.DirectoryName, header.Status);
			return new ErrorResponse(request.CommandName);
		}
		state.LogToServer(Severity.Verbose, "Create Directory: User '{0}' created '{1}{2}'.", session.UserName, share.Name, request.DirectoryName);
		return new CreateDirectoryResponse();
	}

	internal static SMB1Command GetDeleteDirectoryResponse(SMB1Header header, DeleteDirectoryRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, request.DirectoryName))
		{
			state.LogToServer(Severity.Verbose, "Delete Directory '{0}{1}' failed. User '{2}' was denied access.", share.Name, request.DirectoryName, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = SMB1FileStoreHelper.DeleteDirectory(share.FileStore, request.DirectoryName, session.SecurityContext);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Delete Directory '{0}{1}' failed. NTStatus: {2}.", share.Name, request.DirectoryName, header.Status);
			return new ErrorResponse(request.CommandName);
		}
		state.LogToServer(Severity.Verbose, "Delete Directory: User '{0}' deleted '{1}{2}'.", session.UserName, share.Name, request.DirectoryName);
		return new DeleteDirectoryResponse();
	}

	internal static SMB1Command GetDeleteResponse(SMB1Header header, DeleteRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, request.FileName))
		{
			state.LogToServer(Severity.Verbose, "Delete '{0}{1}' failed. User '{2}' was denied access.", share.Name, request.FileName, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = SMB1FileStoreHelper.DeleteFile(share.FileStore, request.FileName, session.SecurityContext);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Delete '{0}{1}' failed. NTStatus: {2}.", share.Name, request.FileName, header.Status);
			return new ErrorResponse(request.CommandName);
		}
		state.LogToServer(Severity.Verbose, "Delete: User '{0}' deleted '{1}{2}'.", session.UserName, share.Name, request.FileName);
		return new DeleteResponse();
	}

	internal static SMB1Command GetRenameResponse(SMB1Header header, RenameRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		if (share is FileSystemShare)
		{
			if (!((FileSystemShare)share).HasWriteAccess(session.SecurityContext, request.OldFileName))
			{
				state.LogToServer(Severity.Verbose, "Rename '{0}{1}' to '{0}{2}' failed. User '{3}' was denied access.", share.Name, request.OldFileName, request.NewFileName, session.UserName);
				header.Status = NTStatus.STATUS_ACCESS_DENIED;
				return new ErrorResponse(request.CommandName);
			}
			if (!((FileSystemShare)share).HasWriteAccess(session.SecurityContext, request.NewFileName))
			{
				state.LogToServer(Severity.Verbose, "Rename '{0}{1}' to '{0}{2}' failed. User '{3}' was denied access.", share.Name, request.OldFileName, request.NewFileName, session.UserName);
				header.Status = NTStatus.STATUS_ACCESS_DENIED;
				return new ErrorResponse(request.CommandName);
			}
		}
		header.Status = SMB1FileStoreHelper.Rename(share.FileStore, request.OldFileName, request.NewFileName, request.SearchAttributes, session.SecurityContext);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Rename '{0}{1}' to '{0}{2}' failed. NTStatus: {3}.", share.Name, request.OldFileName, request.NewFileName, header.Status);
			return new ErrorResponse(request.CommandName);
		}
		state.LogToServer(Severity.Verbose, "Rename: User '{0}' renamed '{1}{2}' to '{1}{3}'.", session.UserName, share.Name, request.OldFileName, request.NewFileName);
		return new RenameResponse();
	}

	internal static SMB1Command GetCheckDirectoryResponse(SMB1Header header, CheckDirectoryRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		string text = request.DirectoryName;
		if (!text.StartsWith("\\"))
		{
			text = "\\" + text;
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, text))
		{
			state.LogToServer(Severity.Verbose, "Check Directory '{0}{1}' failed. User '{2}' was denied access.", share.Name, text, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = SMB1FileStoreHelper.CheckDirectory(share.FileStore, text, session.SecurityContext);
		if (header.Status != 0)
		{
			return new ErrorResponse(request.CommandName);
		}
		return new CheckDirectoryResponse();
	}

	internal static SMB1Command GetQueryInformationResponse(SMB1Header header, QueryInformationRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		string text = request.FileName;
		if (!text.StartsWith("\\"))
		{
			text = "\\" + text;
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, text))
		{
			state.LogToServer(Severity.Verbose, "Query Information on '{0}{1}' failed. User '{2}' was denied access.", share.Name, text, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = SMB1FileStoreHelper.QueryInformation(out var fileInfo, share.FileStore, text, session.SecurityContext);
		if (header.Status != 0)
		{
			return new ErrorResponse(request.CommandName);
		}
		return new QueryInformationResponse
		{
			FileAttributes = SMB1FileStoreHelper.GetFileAttributes(fileInfo.FileAttributes),
			LastWriteTime = fileInfo.LastWriteTime,
			FileSize = (uint)Math.Min(4294967295L, fileInfo.EndOfFile)
		};
	}

	internal static SMB1Command GetSetInformationResponse(SMB1Header header, SetInformationRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, request.FileName))
		{
			state.LogToServer(Severity.Verbose, "Set Information on '{0}{1}' failed. User '{2}' was denied access.", share.Name, request.FileName, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = SMB1FileStoreHelper.SetInformation(share.FileStore, request.FileName, request.FileAttributes, request.LastWriteTime, session.SecurityContext);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Set Information on '{0}{1}' failed. NTStatus: {2}", share.Name, request.FileName, header.Status);
			return new ErrorResponse(request.CommandName);
		}
		state.LogToServer(Severity.Verbose, "Set Information on '{0}{1}' succeeded.", share.Name, request.FileName);
		return new SetInformationResponse();
	}

	internal static SMB1Command GetSetInformation2Response(SMB1Header header, SetInformation2Request request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Set Information 2 failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, request.FID);
			header.Status = NTStatus.STATUS_SMB_BAD_FID;
			return new ErrorResponse(request.CommandName);
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "Set Information 2 on '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = SMB1FileStoreHelper.SetInformation2(share.FileStore, openFileObject.Handle, request.CreationDateTime, request.LastAccessDateTime, request.LastWriteDateTime);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Set Information 2 on '{0}{1}' failed. NTStatus: {2}", share.Name, openFileObject.Path, header.Status);
			return new ErrorResponse(request.CommandName);
		}
		state.LogToServer(Severity.Verbose, "Set Information 2 on '{0}{1}' succeeded.", share.Name, openFileObject.Path);
		return new SetInformation2Response();
	}
}
