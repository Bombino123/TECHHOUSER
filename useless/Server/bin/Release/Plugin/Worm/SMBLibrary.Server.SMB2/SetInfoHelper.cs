using System;
using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class SetInfoHelper
{
	internal static SMB2Command GetSetInfoResponse(SetInfoRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		OpenFileObject openFileObject = null;
		if (request.InfoType == InfoType.File || request.InfoType == InfoType.Security)
		{
			openFileObject = session.GetOpenFileObject(request.FileId);
			if (openFileObject == null)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
			}
			if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, openFileObject.Path))
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
			}
		}
		else if (request.InfoType == InfoType.FileSystem && share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, "\\"))
		{
			state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. User '{1}' was denied access.", share.Name, session.UserName);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
		}
		if (request.InfoType == InfoType.File)
		{
			FileInformation fileInformation;
			try
			{
				fileInformation = FileInformation.GetFileInformation(request.Buffer, 0, request.FileInformationClass);
			}
			catch (UnsupportedInformationLevelException)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: STATUS_INVALID_INFO_CLASS.", share.Name, openFileObject.Path, request.FileInformationClass);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_INFO_CLASS);
			}
			catch (NotImplementedException)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: STATUS_NOT_SUPPORTED.", share.Name, openFileObject.Path, request.FileInformationClass);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_NOT_SUPPORTED);
			}
			catch (Exception)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: STATUS_INVALID_PARAMETER.", share.Name, openFileObject.Path, request.FileInformationClass);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
			}
			if (share is FileSystemShare && fileInformation is FileRenameInformationType2)
			{
				string text = ((FileRenameInformationType2)fileInformation).FileName;
				if (!text.StartsWith("\\"))
				{
					text = "\\" + text;
				}
				if (!((FileSystemShare)share).HasWriteAccess(session.SecurityContext, text))
				{
					state.LogToServer(Severity.Verbose, "SetFileInformation: Rename '{0}{1}' to '{0}{2}' failed. User '{3}' was denied access.", share.Name, openFileObject.Path, text, session.UserName);
					return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
				}
			}
			NTStatus nTStatus = share.FileStore.SetFileInformation(openFileObject.Handle, fileInformation);
			if (nTStatus != 0)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: {3}. (FileId: {4})", share.Name, openFileObject.Path, request.FileInformationClass, nTStatus, request.FileId.Volatile);
				return new ErrorResponse(request.CommandName, nTStatus);
			}
			if (fileInformation is FileRenameInformationType2)
			{
				string text2 = ((FileRenameInformationType2)fileInformation).FileName;
				if (!text2.StartsWith("\\"))
				{
					text2 = "\\" + text2;
				}
				state.LogToServer(Severity.Verbose, "SetFileInformation: Rename '{0}{1}' to '{0}{2}' succeeded. (FileId: {3})", share.Name, openFileObject.Path, text2, request.FileId.Volatile);
				openFileObject.Path = text2;
			}
			else
			{
				state.LogToServer(Severity.Information, "SetFileInformation on '{0}{1}' succeeded. Information class: {2}. (FileId: {3})", share.Name, openFileObject.Path, request.FileInformationClass, request.FileId.Volatile);
			}
			return new SetInfoResponse();
		}
		if (request.InfoType == InfoType.FileSystem)
		{
			FileSystemInformation fileSystemInformation;
			try
			{
				fileSystemInformation = FileSystemInformation.GetFileSystemInformation(request.Buffer, 0, request.FileSystemInformationClass);
			}
			catch (UnsupportedInformationLevelException)
			{
				state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. Information class: {1}, NTStatus: STATUS_INVALID_INFO_CLASS.", share.Name, request.FileSystemInformationClass);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_INFO_CLASS);
			}
			catch (Exception)
			{
				state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. Information class: {1}, NTStatus: STATUS_INVALID_PARAMETER.", share.Name, request.FileSystemInformationClass);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
			}
			NTStatus nTStatus2 = share.FileStore.SetFileSystemInformation(fileSystemInformation);
			if (nTStatus2 != 0)
			{
				state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. Information class: {1}, NTStatus: {2}.", share.Name, request.FileSystemInformationClass, nTStatus2);
				return new ErrorResponse(request.CommandName, nTStatus2);
			}
			state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' succeeded. Information class: {1}.", share.Name, request.FileSystemInformationClass);
			return new SetInfoResponse();
		}
		if (request.InfoType == InfoType.Security)
		{
			SecurityDescriptor securityDescriptor;
			try
			{
				securityDescriptor = new SecurityDescriptor(request.Buffer, 0);
			}
			catch
			{
				state.LogToServer(Severity.Verbose, "SetSecurityInformation on '{0}{1}' failed. NTStatus: STATUS_INVALID_PARAMETER.", share.Name, openFileObject.Path);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
			}
			NTStatus nTStatus3 = share.FileStore.SetSecurityInformation(openFileObject, request.SecurityInformation, securityDescriptor);
			if (nTStatus3 != 0)
			{
				state.LogToServer(Severity.Verbose, "SetSecurityInformation on '{0}{1}' failed. Security information: 0x{2}, NTStatus: {3}. (FileId: {4})", share.Name, openFileObject.Path, request.SecurityInformation.ToString("X"), nTStatus3, request.FileId.Volatile);
				return new ErrorResponse(request.CommandName, nTStatus3);
			}
			state.LogToServer(Severity.Information, "SetSecurityInformation on '{0}{1}' succeeded. Security information: 0x{2}. (FileId: {3})", share.Name, openFileObject.Path, request.SecurityInformation.ToString("X"), request.FileId.Volatile);
			return new SetInfoResponse();
		}
		return new ErrorResponse(request.CommandName, NTStatus.STATUS_NOT_SUPPORTED);
	}
}
