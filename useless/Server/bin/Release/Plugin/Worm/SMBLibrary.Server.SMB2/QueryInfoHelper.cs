using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class QueryInfoHelper
{
	internal static SMB2Command GetQueryInfoResponse(QueryInfoRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		if (request.InfoType == InfoType.File)
		{
			OpenFileObject openFileObject = session.GetOpenFileObject(request.FileId);
			if (openFileObject == null)
			{
				state.LogToServer(Severity.Verbose, "GetFileInformation failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
			}
			if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, openFileObject.Path))
			{
				state.LogToServer(Severity.Verbose, "GetFileInformation on '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
			}
			FileInformation result;
			NTStatus fileInformation = share.FileStore.GetFileInformation(out result, openFileObject.Handle, request.FileInformationClass);
			if (fileInformation != 0)
			{
				state.LogToServer(Severity.Verbose, "GetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: {3}. (FileId: {4})", share.Name, openFileObject.Path, request.FileInformationClass, fileInformation, request.FileId.Volatile);
				return new ErrorResponse(request.CommandName, fileInformation);
			}
			state.LogToServer(Severity.Information, "GetFileInformation on '{0}{1}' succeeded. Information class: {2}. (FileId: {3})", share.Name, openFileObject.Path, request.FileInformationClass, request.FileId.Volatile);
			QueryInfoResponse queryInfoResponse = new QueryInfoResponse();
			queryInfoResponse.SetFileInformation(result);
			if (queryInfoResponse.OutputBuffer.Length > request.OutputBufferLength)
			{
				queryInfoResponse.Header.Status = NTStatus.STATUS_BUFFER_OVERFLOW;
				queryInfoResponse.OutputBuffer = ByteReader.ReadBytes(queryInfoResponse.OutputBuffer, 0, (int)request.OutputBufferLength);
			}
			return queryInfoResponse;
		}
		if (request.InfoType == InfoType.FileSystem)
		{
			if (share is FileSystemShare)
			{
				if (!((FileSystemShare)share).HasReadAccess(session.SecurityContext, "\\"))
				{
					state.LogToServer(Severity.Verbose, "GetFileSystemInformation on '{0}' failed. User '{1}' was denied access.", share.Name, session.UserName);
					return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
				}
				FileSystemInformation result2;
				NTStatus fileSystemInformation = share.FileStore.GetFileSystemInformation(out result2, request.FileSystemInformationClass);
				if (fileSystemInformation != 0)
				{
					state.LogToServer(Severity.Verbose, "GetFileSystemInformation on '{0}' failed. Information class: {1}, NTStatus: {2}", share.Name, request.FileSystemInformationClass, fileSystemInformation);
					return new ErrorResponse(request.CommandName, fileSystemInformation);
				}
				state.LogToServer(Severity.Information, "GetFileSystemInformation on '{0}' succeeded. Information class: {1}", share.Name, request.FileSystemInformationClass);
				QueryInfoResponse queryInfoResponse2 = new QueryInfoResponse();
				queryInfoResponse2.SetFileSystemInformation(result2);
				if (queryInfoResponse2.OutputBuffer.Length > request.OutputBufferLength)
				{
					queryInfoResponse2.Header.Status = NTStatus.STATUS_BUFFER_OVERFLOW;
					queryInfoResponse2.OutputBuffer = ByteReader.ReadBytes(queryInfoResponse2.OutputBuffer, 0, (int)request.OutputBufferLength);
				}
				return queryInfoResponse2;
			}
		}
		else if (request.InfoType == InfoType.Security)
		{
			OpenFileObject openFileObject2 = session.GetOpenFileObject(request.FileId);
			if (openFileObject2 == null)
			{
				state.LogToServer(Severity.Verbose, "GetSecurityInformation failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
			}
			if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, openFileObject2.Path))
			{
				state.LogToServer(Severity.Verbose, "GetSecurityInformation on '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject2.Path, session.UserName);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
			}
			SecurityDescriptor result3;
			NTStatus securityInformation = share.FileStore.GetSecurityInformation(out result3, openFileObject2.Handle, request.SecurityInformation);
			if (securityInformation != 0)
			{
				state.LogToServer(Severity.Verbose, "GetSecurityInformation on '{0}{1}' failed. Security information: 0x{2}, NTStatus: {3}. (FileId: {4})", share.Name, openFileObject2.Path, request.SecurityInformation.ToString("X"), securityInformation, request.FileId.Volatile);
				return new ErrorResponse(request.CommandName, securityInformation);
			}
			if (result3.Length > request.OutputBufferLength)
			{
				state.LogToServer(Severity.Information, "GetSecurityInformation on '{0}{1}' failed. Security information: 0x{2}, NTStatus: STATUS_BUFFER_TOO_SMALL. (FileId: {3})", share.Name, openFileObject2.Path, request.SecurityInformation.ToString("X"), request.FileId.Volatile);
				byte[] bytes = LittleEndianConverter.GetBytes((uint)result3.Length);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_BUFFER_TOO_SMALL, bytes);
			}
			state.LogToServer(Severity.Information, "GetSecurityInformation on '{0}{1}' succeeded. Security information: 0x{2}. (FileId: {3})", share.Name, openFileObject2.Path, request.SecurityInformation.ToString("X"), request.FileId.Volatile);
			QueryInfoResponse queryInfoResponse3 = new QueryInfoResponse();
			queryInfoResponse3.SetSecurityInformation(result3);
			return queryInfoResponse3;
		}
		return new ErrorResponse(request.CommandName, NTStatus.STATUS_NOT_SUPPORTED);
	}
}
