using System;
using System.Collections.Generic;
using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class QueryDirectoryHelper
{
	internal static SMB2Command GetQueryDirectoryResponse(QueryDirectoryRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FileId);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Query Directory failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
		}
		if (!((FileSystemShare)share).HasReadAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "Query Directory on '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
		}
		_ = (FileSystemShare)share;
		FileID fileId = request.FileId;
		OpenSearch openSearch = session.GetOpenSearch(fileId);
		if (openSearch == null || request.Reopen)
		{
			if (request.Reopen)
			{
				session.RemoveOpenSearch(fileId);
			}
			List<QueryDirectoryFileInformation> result;
			NTStatus nTStatus = share.FileStore.QueryDirectory(out result, openFileObject.Handle, request.FileName, request.FileInformationClass);
			if (nTStatus != 0)
			{
				state.LogToServer(Severity.Verbose, "Query Directory on '{0}{1}', Searched for '{2}', NTStatus: {3}", share.Name, openFileObject.Path, request.FileName, nTStatus.ToString());
				return new ErrorResponse(request.CommandName, nTStatus);
			}
			state.LogToServer(Severity.Information, "Query Directory on '{0}{1}', Searched for '{2}', found {3} matching entries", share.Name, openFileObject.Path, request.FileName, result.Count);
			openSearch = session.AddOpenSearch(fileId, result, 0);
		}
		if (request.Restart || request.Reopen)
		{
			openSearch.EnumerationLocation = 0;
		}
		if (openSearch.Entries.Count == 0)
		{
			session.RemoveOpenSearch(fileId);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_NO_SUCH_FILE);
		}
		if (openSearch.EnumerationLocation == openSearch.Entries.Count)
		{
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_NO_MORE_FILES);
		}
		List<QueryDirectoryFileInformation> list = new List<QueryDirectoryFileInformation>();
		int num = 0;
		for (int i = openSearch.EnumerationLocation; i < openSearch.Entries.Count; i++)
		{
			QueryDirectoryFileInformation queryDirectoryFileInformation = openSearch.Entries[i];
			if (queryDirectoryFileInformation.FileInformationClass != request.FileInformationClass)
			{
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
			}
			int length = queryDirectoryFileInformation.Length;
			if (num + length > request.OutputBufferLength)
			{
				break;
			}
			list.Add(queryDirectoryFileInformation);
			int num2 = (int)Math.Ceiling((double)length / 8.0) * 8;
			num += num2;
			openSearch.EnumerationLocation = i + 1;
			if (request.ReturnSingleEntry)
			{
				break;
			}
		}
		QueryDirectoryResponse queryDirectoryResponse = new QueryDirectoryResponse();
		queryDirectoryResponse.SetFileInformationList(list);
		return queryDirectoryResponse;
	}
}
