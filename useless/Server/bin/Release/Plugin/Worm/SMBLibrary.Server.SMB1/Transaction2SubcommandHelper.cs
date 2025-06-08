using System;
using System.Collections.Generic;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class Transaction2SubcommandHelper
{
	internal static Transaction2FindFirst2Response GetSubcommandResponse(SMB1Header header, uint maxDataCount, Transaction2FindFirst2Request subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		string text = subcommand.FileName;
		if (!text.StartsWith("\\"))
		{
			text = "\\" + text;
		}
		FileInformationClass fileInformation;
		try
		{
			fileInformation = FindInformationHelper.ToFileInformationClass(subcommand.InformationLevel);
		}
		catch (UnsupportedInformationLevelException)
		{
			state.LogToServer(Severity.Verbose, "FindFirst2: Unsupported information level: {0}.", subcommand.InformationLevel);
			header.Status = NTStatus.STATUS_OS2_INVALID_LEVEL;
			return null;
		}
		List<QueryDirectoryFileInformation> result;
		NTStatus nTStatus = SMB1FileStoreHelper.QueryDirectory(out result, share.FileStore, text, fileInformation, session.SecurityContext);
		if (nTStatus != 0)
		{
			state.LogToServer(Severity.Verbose, "FindFirst2: Searched for '{0}{1}', NTStatus: {2}", share.Name, text, nTStatus.ToString());
			header.Status = nTStatus;
			return null;
		}
		state.LogToServer(Severity.Information, "FindFirst2: Searched for '{0}{1}', found {2} matching entries", share.Name, text, result.Count);
		if (result.Count == 0)
		{
			header.Status = NTStatus.STATUS_NO_SUCH_FILE;
			return null;
		}
		_ = subcommand.Flags;
		int count = Math.Min(subcommand.SearchCount, result.Count);
		List<QueryDirectoryFileInformation> range = result.GetRange(0, count);
		FindInformationList findInformationList;
		try
		{
			findInformationList = FindInformationHelper.ToFindInformationList(range, header.UnicodeFlag, (int)maxDataCount);
		}
		catch (UnsupportedInformationLevelException)
		{
			state.LogToServer(Severity.Verbose, "FindFirst2: Unsupported information level: {0}.", subcommand.InformationLevel);
			header.Status = NTStatus.STATUS_OS2_INVALID_LEVEL;
			return null;
		}
		int count2 = findInformationList.Count;
		Transaction2FindFirst2Response transaction2FindFirst2Response = new Transaction2FindFirst2Response();
		transaction2FindFirst2Response.SetFindInformationList(findInformationList, header.UnicodeFlag);
		transaction2FindFirst2Response.EndOfSearch = count2 == result.Count;
		if ((transaction2FindFirst2Response.EndOfSearch && subcommand.CloseAtEndOfSearch) || subcommand.CloseAfterRequest)
		{
			transaction2FindFirst2Response.SID = 0;
		}
		else
		{
			ushort? num = session.AddOpenSearch(result, count2);
			if (!num.HasValue)
			{
				header.Status = NTStatus.STATUS_OS2_NO_MORE_SIDS;
				return null;
			}
			transaction2FindFirst2Response.SID = num.Value;
		}
		return transaction2FindFirst2Response;
	}

	internal static Transaction2FindNext2Response GetSubcommandResponse(SMB1Header header, uint maxDataCount, Transaction2FindNext2Request subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		OpenSearch openSearch = session.GetOpenSearch(subcommand.SID);
		if (openSearch == null)
		{
			state.LogToServer(Severity.Verbose, "FindNext2 failed. Invalid SID.");
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return null;
		}
		_ = subcommand.Flags;
		int count = Math.Min(openSearch.Entries.Count - openSearch.EnumerationLocation, subcommand.SearchCount);
		List<QueryDirectoryFileInformation> range = openSearch.Entries.GetRange(openSearch.EnumerationLocation, count);
		FindInformationList findInformationList;
		try
		{
			findInformationList = FindInformationHelper.ToFindInformationList(range, header.UnicodeFlag, (int)maxDataCount);
		}
		catch (UnsupportedInformationLevelException)
		{
			state.LogToServer(Severity.Verbose, "FindNext2: Unsupported information level: {0}.", subcommand.InformationLevel);
			header.Status = NTStatus.STATUS_OS2_INVALID_LEVEL;
			return null;
		}
		int count2 = findInformationList.Count;
		Transaction2FindNext2Response transaction2FindNext2Response = new Transaction2FindNext2Response();
		transaction2FindNext2Response.SetFindInformationList(findInformationList, header.UnicodeFlag);
		openSearch.EnumerationLocation += count2;
		transaction2FindNext2Response.EndOfSearch = openSearch.EnumerationLocation == openSearch.Entries.Count;
		if (transaction2FindNext2Response.EndOfSearch)
		{
			session.RemoveOpenSearch(subcommand.SID);
		}
		return transaction2FindNext2Response;
	}

	internal static Transaction2QueryFSInformationResponse GetSubcommandResponse(SMB1Header header, uint maxDataCount, Transaction2QueryFSInformationRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, "\\"))
		{
			state.LogToServer(Severity.Verbose, "QueryFileSystemInformation on '{0}' failed. User '{1}' was denied access.", share.Name, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return null;
		}
		Transaction2QueryFSInformationResponse transaction2QueryFSInformationResponse = new Transaction2QueryFSInformationResponse();
		if (subcommand.IsPassthroughInformationLevel)
		{
			FileSystemInformation result;
			NTStatus fileSystemInformation = share.FileStore.GetFileSystemInformation(out result, subcommand.FileSystemInformationClass);
			if (fileSystemInformation != 0)
			{
				state.LogToServer(Severity.Verbose, "GetFileSystemInformation on '{0}' failed. Information class: {1}, NTStatus: {2}", share.Name, subcommand.FileSystemInformationClass, fileSystemInformation);
				header.Status = fileSystemInformation;
				return null;
			}
			state.LogToServer(Severity.Information, "GetFileSystemInformation on '{0}' succeeded. Information class: {1}", share.Name, subcommand.FileSystemInformationClass);
			transaction2QueryFSInformationResponse.SetFileSystemInformation(result);
		}
		else
		{
			QueryFSInformation result2;
			NTStatus fileSystemInformation2 = SMB1FileStoreHelper.GetFileSystemInformation(out result2, share.FileStore, subcommand.QueryFSInformationLevel);
			if (fileSystemInformation2 != 0)
			{
				state.LogToServer(Severity.Verbose, "GetFileSystemInformation on '{0}' failed. Information level: {1}, NTStatus: {2}", share.Name, subcommand.QueryFSInformationLevel, fileSystemInformation2);
				header.Status = fileSystemInformation2;
				return null;
			}
			state.LogToServer(Severity.Information, "GetFileSystemInformation on '{0}' succeeded. Information level: {1}", share.Name, subcommand.QueryFSInformationLevel);
			transaction2QueryFSInformationResponse.SetQueryFSInformation(result2, header.UnicodeFlag);
		}
		if (transaction2QueryFSInformationResponse.InformationBytes.Length > maxDataCount)
		{
			header.Status = NTStatus.STATUS_BUFFER_OVERFLOW;
			transaction2QueryFSInformationResponse.InformationBytes = ByteReader.ReadBytes(transaction2QueryFSInformationResponse.InformationBytes, 0, (int)maxDataCount);
		}
		return transaction2QueryFSInformationResponse;
	}

	internal static Transaction2SetFSInformationResponse GetSubcommandResponse(SMB1Header header, Transaction2SetFSInformationRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, "\\"))
		{
			state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. User '{1}' was denied access.", share.Name, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return null;
		}
		if (!subcommand.IsPassthroughInformationLevel)
		{
			state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. Not a pass-through information level.", share.Name);
			header.Status = NTStatus.STATUS_NOT_SUPPORTED;
			return null;
		}
		FileSystemInformation fileSystemInformation;
		try
		{
			fileSystemInformation = FileSystemInformation.GetFileSystemInformation(subcommand.InformationBytes, 0, subcommand.FileSystemInformationClass);
		}
		catch (UnsupportedInformationLevelException)
		{
			state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. Information class: {1}, NTStatus: STATUS_OS2_INVALID_LEVEL.", share.Name, subcommand.FileSystemInformationClass);
			header.Status = NTStatus.STATUS_OS2_INVALID_LEVEL;
			return null;
		}
		catch (Exception)
		{
			state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. Information class: {1}, NTStatus: STATUS_INVALID_PARAMETER.", share.Name, subcommand.FileSystemInformationClass);
			header.Status = NTStatus.STATUS_INVALID_PARAMETER;
			return null;
		}
		NTStatus nTStatus = share.FileStore.SetFileSystemInformation(fileSystemInformation);
		if (nTStatus != 0)
		{
			state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' failed. Information class: {1}, NTStatus: {2}.", share.Name, subcommand.FileSystemInformationClass, nTStatus);
			header.Status = nTStatus;
			return null;
		}
		state.LogToServer(Severity.Verbose, "SetFileSystemInformation on '{0}' succeeded. Information class: {1}.", share.Name, subcommand.FileSystemInformationClass);
		return new Transaction2SetFSInformationResponse();
	}

	internal static Transaction2QueryPathInformationResponse GetSubcommandResponse(SMB1Header header, uint maxDataCount, Transaction2QueryPathInformationRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		string text = subcommand.FileName;
		if (!text.StartsWith("\\"))
		{
			text = "\\" + text;
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, text))
		{
			state.LogToServer(Severity.Verbose, "QueryPathInformation on '{0}{1}' failed. User '{2}' was denied access.", share.Name, text, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return null;
		}
		Transaction2QueryPathInformationResponse transaction2QueryPathInformationResponse = new Transaction2QueryPathInformationResponse();
		if (subcommand.IsPassthroughInformationLevel && subcommand.FileInformationClass != FileInformationClass.FileAllInformation)
		{
			FileInformation result;
			NTStatus fileInformation = SMB1FileStoreHelper.GetFileInformation(out result, share.FileStore, text, subcommand.FileInformationClass, session.SecurityContext);
			if (fileInformation != 0)
			{
				state.LogToServer(Severity.Verbose, "GetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: {3}", share.Name, text, subcommand.FileInformationClass, fileInformation);
				header.Status = fileInformation;
				return null;
			}
			state.LogToServer(Severity.Information, "GetFileInformation on '{0}{1}' succeeded. Information class: {2}", share.Name, text, subcommand.FileInformationClass);
			transaction2QueryPathInformationResponse.SetFileInformation(result);
		}
		else
		{
			if (subcommand.IsPassthroughInformationLevel && subcommand.FileInformationClass == FileInformationClass.FileAllInformation)
			{
				subcommand.QueryInformationLevel = QueryInformationLevel.SMB_QUERY_FILE_ALL_INFO;
			}
			QueryInformation result2;
			NTStatus fileInformation2 = SMB1FileStoreHelper.GetFileInformation(out result2, share.FileStore, text, subcommand.QueryInformationLevel, session.SecurityContext);
			if (fileInformation2 != 0)
			{
				state.LogToServer(Severity.Verbose, "GetFileInformation on '{0}{1}' failed. Information level: {2}, NTStatus: {3}", share.Name, text, subcommand.QueryInformationLevel, fileInformation2);
				header.Status = fileInformation2;
				return null;
			}
			state.LogToServer(Severity.Information, "GetFileInformation on '{0}{1}' succeeded. Information level: {2}", share.Name, text, subcommand.QueryInformationLevel);
			transaction2QueryPathInformationResponse.SetQueryInformation(result2);
		}
		if (transaction2QueryPathInformationResponse.InformationBytes.Length > maxDataCount)
		{
			header.Status = NTStatus.STATUS_BUFFER_OVERFLOW;
			transaction2QueryPathInformationResponse.InformationBytes = ByteReader.ReadBytes(transaction2QueryPathInformationResponse.InformationBytes, 0, (int)maxDataCount);
		}
		return transaction2QueryPathInformationResponse;
	}

	internal static Transaction2QueryFileInformationResponse GetSubcommandResponse(SMB1Header header, uint maxDataCount, Transaction2QueryFileInformationRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		OpenFileObject openFileObject = session.GetOpenFileObject(subcommand.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "QueryFileInformation failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, subcommand.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return null;
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "QueryFileInformation on '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return null;
		}
		Transaction2QueryFileInformationResponse transaction2QueryFileInformationResponse = new Transaction2QueryFileInformationResponse();
		if (subcommand.IsPassthroughInformationLevel && subcommand.FileInformationClass != FileInformationClass.FileAllInformation)
		{
			FileInformation result;
			NTStatus fileInformation = share.FileStore.GetFileInformation(out result, openFileObject.Handle, subcommand.FileInformationClass);
			if (fileInformation != 0)
			{
				state.LogToServer(Severity.Verbose, "GetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: {3}. (FID: {4})", share.Name, openFileObject.Path, subcommand.FileInformationClass, fileInformation, subcommand.FID);
				header.Status = fileInformation;
				return null;
			}
			state.LogToServer(Severity.Information, "GetFileInformation on '{0}{1}' succeeded. Information class: {2}. (FID: {3})", share.Name, openFileObject.Path, subcommand.FileInformationClass, subcommand.FID);
			transaction2QueryFileInformationResponse.SetFileInformation(result);
		}
		else
		{
			if (subcommand.IsPassthroughInformationLevel && subcommand.FileInformationClass == FileInformationClass.FileAllInformation)
			{
				subcommand.QueryInformationLevel = QueryInformationLevel.SMB_QUERY_FILE_ALL_INFO;
			}
			QueryInformation result2;
			NTStatus fileInformation2 = SMB1FileStoreHelper.GetFileInformation(out result2, share.FileStore, openFileObject.Handle, subcommand.QueryInformationLevel);
			if (fileInformation2 != 0)
			{
				state.LogToServer(Severity.Verbose, "GetFileInformation on '{0}{1}' failed. Information level: {2}, NTStatus: {3}. (FID: {4})", share.Name, openFileObject.Path, subcommand.QueryInformationLevel, fileInformation2, subcommand.FID);
				header.Status = fileInformation2;
				return null;
			}
			state.LogToServer(Severity.Information, "GetFileInformation on '{0}{1}' succeeded. Information level: {2}. (FID: {3})", share.Name, openFileObject.Path, subcommand.QueryInformationLevel, subcommand.FID);
			transaction2QueryFileInformationResponse.SetQueryInformation(result2);
		}
		if (transaction2QueryFileInformationResponse.InformationBytes.Length > maxDataCount)
		{
			header.Status = NTStatus.STATUS_BUFFER_OVERFLOW;
			transaction2QueryFileInformationResponse.InformationBytes = ByteReader.ReadBytes(transaction2QueryFileInformationResponse.InformationBytes, 0, (int)maxDataCount);
		}
		return transaction2QueryFileInformationResponse;
	}

	internal static Transaction2SetFileInformationResponse GetSubcommandResponse(SMB1Header header, Transaction2SetFileInformationRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		OpenFileObject openFileObject = session.GetOpenFileObject(subcommand.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "SetFileInformation failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, subcommand.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return null;
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return null;
		}
		if (subcommand.IsPassthroughInformationLevel)
		{
			FileInformation fileInformation;
			try
			{
				fileInformation = FileInformation.GetFileInformation(subcommand.InformationBytes, 0, subcommand.FileInformationClass);
			}
			catch (UnsupportedInformationLevelException)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: STATUS_OS2_INVALID_LEVEL.", share.Name, openFileObject.Path, subcommand.FileInformationClass);
				header.Status = NTStatus.STATUS_OS2_INVALID_LEVEL;
				return null;
			}
			catch (Exception)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: STATUS_INVALID_PARAMETER.", share.Name, openFileObject.Path, subcommand.FileInformationClass);
				header.Status = NTStatus.STATUS_INVALID_PARAMETER;
				return null;
			}
			NTStatus nTStatus = share.FileStore.SetFileInformation(openFileObject.Handle, fileInformation);
			if (nTStatus != 0)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information class: {2}, NTStatus: {3}. (FID: {4})", share.Name, openFileObject.Path, subcommand.FileInformationClass, nTStatus, subcommand.FID);
				header.Status = nTStatus;
				return null;
			}
			state.LogToServer(Severity.Information, "SetFileInformation on '{0}{1}' succeeded. Information class: {2}. (FID: {3})", share.Name, openFileObject.Path, subcommand.FileInformationClass, subcommand.FID);
		}
		else
		{
			SetInformation setInformation;
			try
			{
				setInformation = SetInformation.GetSetInformation(subcommand.InformationBytes, subcommand.SetInformationLevel);
			}
			catch (UnsupportedInformationLevelException)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information level: {2}, NTStatus: STATUS_OS2_INVALID_LEVEL.", share.Name, openFileObject.Path, subcommand.SetInformationLevel);
				header.Status = NTStatus.STATUS_OS2_INVALID_LEVEL;
				return null;
			}
			catch (Exception)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information level: {2}, NTStatus: STATUS_INVALID_PARAMETER.", share.Name, openFileObject.Path, subcommand.SetInformationLevel);
				header.Status = NTStatus.STATUS_INVALID_PARAMETER;
				return null;
			}
			NTStatus nTStatus2 = SMB1FileStoreHelper.SetFileInformation(share.FileStore, openFileObject.Handle, setInformation);
			if (nTStatus2 != 0)
			{
				state.LogToServer(Severity.Verbose, "SetFileInformation on '{0}{1}' failed. Information level: {2}, NTStatus: {3}. (FID: {4})", share.Name, openFileObject.Path, subcommand.SetInformationLevel, nTStatus2, subcommand.FID);
				header.Status = nTStatus2;
				return null;
			}
			state.LogToServer(Severity.Information, "SetFileInformation on '{0}{1}' succeeded. Information level: {2}. (FID: {3})", share.Name, openFileObject.Path, subcommand.SetInformationLevel, subcommand.FID);
		}
		return new Transaction2SetFileInformationResponse();
	}
}
