using System;
using System.IO;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class OpenAndXHelper
{
	internal static SMB1Command GetOpenAndXResponse(SMB1Header header, OpenAndXRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		bool flag = (int)(request.Flags & OpenFlags.SMB_OPEN_EXTENDED_RESPONSE) > 0;
		string text = request.FileName;
		if (!text.StartsWith("\\"))
		{
			text = "\\" + text;
		}
		AccessMask desiredAccess;
		ShareAccess shareAccess;
		CreateDisposition createDisposition;
		try
		{
			desiredAccess = ToAccessMask(request.AccessMode.AccessMode);
			shareAccess = ToShareAccess(request.AccessMode.SharingMode);
			createDisposition = ToCreateDisposition(request.OpenMode);
		}
		catch (ArgumentException)
		{
			header.Status = NTStatus.STATUS_OS2_INVALID_ACCESS;
			return new ErrorResponse(request.CommandName);
		}
		CreateOptions createOptions = ToCreateOptions(request.AccessMode);
		FileAccess requestedAccess = NTFileStoreHelper.ToCreateFileAccess(desiredAccess, createDisposition);
		if (share is FileSystemShare && !((FileSystemShare)share).HasAccess(session.SecurityContext, text, requestedAccess))
		{
			state.LogToServer(Severity.Verbose, "OpenAndX: Opening '{0}{1}' failed. User '{2}' was denied access.", share.Name, request.FileName, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = share.FileStore.CreateFile(out var handle, out var fileStatus, text, desiredAccess, (FileAttributes)0u, shareAccess, createDisposition, createOptions, session.SecurityContext);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "OpenAndX: Opening '{0}{1}' failed. NTStatus: {2}.", share.Name, text, header.Status);
			return new ErrorResponse(request.CommandName);
		}
		FileAccess fileAccess = ToFileAccess(request.AccessMode.AccessMode);
		ushort? num = session.AddOpenFile(header.TID, share.Name, text, handle, fileAccess);
		if (!num.HasValue)
		{
			share.FileStore.CloseFile(handle);
			state.LogToServer(Severity.Verbose, "Create: Opening '{0}{1}' failed. Too many open files.", share.Name, text);
			header.Status = NTStatus.STATUS_TOO_MANY_OPENED_FILES;
			return new ErrorResponse(request.CommandName);
		}
		state.LogToServer(Severity.Verbose, "OpenAndX: Opened '{0}{1}'. (UID: {2}, TID: {3}, FID: {4})", share.Name, text, header.UID, header.TID, num.Value);
		OpenResult openResult = ToOpenResult(fileStatus);
		if (share is NamedPipeShare)
		{
			if (flag)
			{
				return CreateResponseExtendedForNamedPipe(num.Value, openResult);
			}
			return CreateResponseForNamedPipe(num.Value, openResult);
		}
		FileNetworkOpenInformation networkOpenInformation = NTFileStoreHelper.GetNetworkOpenInformation(share.FileStore, handle);
		if (flag)
		{
			return CreateResponseExtendedFromFileInfo(networkOpenInformation, num.Value, openResult);
		}
		return CreateResponseFromFileInfo(networkOpenInformation, num.Value, openResult);
	}

	private static AccessMask ToAccessMask(AccessMode accessMode)
	{
		return accessMode switch
		{
			AccessMode.Read => AccessMask.GENERIC_READ, 
			AccessMode.Write => (AccessMask)1073741952u, 
			AccessMode.ReadWrite => AccessMask.GENERIC_WRITE | AccessMask.GENERIC_READ, 
			AccessMode.Execute => AccessMask.GENERIC_EXECUTE | AccessMask.GENERIC_READ, 
			_ => throw new ArgumentException("Invalid AccessMode value"), 
		};
	}

	private static FileAccess ToFileAccess(AccessMode accessMode)
	{
		return accessMode switch
		{
			AccessMode.Write => FileAccess.Write, 
			AccessMode.ReadWrite => FileAccess.ReadWrite, 
			_ => FileAccess.Read, 
		};
	}

	private static ShareAccess ToShareAccess(SharingMode sharingMode)
	{
		return sharingMode switch
		{
			SharingMode.Compatibility => ShareAccess.Read, 
			SharingMode.DenyReadWriteExecute => ShareAccess.None, 
			SharingMode.DenyWrite => ShareAccess.Read, 
			SharingMode.DenyReadExecute => ShareAccess.Write, 
			SharingMode.DenyNothing => ShareAccess.Read | ShareAccess.Write, 
			(SharingMode)255 => ShareAccess.None, 
			_ => throw new ArgumentException("Invalid SharingMode value"), 
		};
	}

	private static CreateDisposition ToCreateDisposition(OpenMode openMode)
	{
		if (openMode.CreateFile == CreateFile.ReturnErrorIfNotExist)
		{
			if (openMode.FileExistsOpts == FileExistsOpts.ReturnError)
			{
				throw new ArgumentException("Invalid OpenMode combination");
			}
			if (openMode.FileExistsOpts == FileExistsOpts.Append)
			{
				return CreateDisposition.FILE_OPEN;
			}
			if (openMode.FileExistsOpts == FileExistsOpts.TruncateToZero)
			{
				return CreateDisposition.FILE_OVERWRITE;
			}
		}
		else if (openMode.CreateFile == CreateFile.CreateIfNotExist)
		{
			if (openMode.FileExistsOpts == FileExistsOpts.ReturnError)
			{
				return CreateDisposition.FILE_CREATE;
			}
			if (openMode.FileExistsOpts == FileExistsOpts.Append)
			{
				return CreateDisposition.FILE_OPEN_IF;
			}
			if (openMode.FileExistsOpts == FileExistsOpts.TruncateToZero)
			{
				return CreateDisposition.FILE_OVERWRITE_IF;
			}
		}
		throw new ArgumentException("Invalid OpenMode combination");
	}

	private static CreateOptions ToCreateOptions(AccessModeOptions accessModeOptions)
	{
		CreateOptions createOptions = CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_COMPLETE_IF_OPLOCKED;
		if (accessModeOptions.ReferenceLocality == ReferenceLocality.Sequential)
		{
			createOptions |= CreateOptions.FILE_SEQUENTIAL_ONLY;
		}
		else if (accessModeOptions.ReferenceLocality == ReferenceLocality.Random)
		{
			createOptions |= CreateOptions.FILE_RANDOM_ACCESS;
		}
		else if (accessModeOptions.ReferenceLocality == ReferenceLocality.RandomWithLocality)
		{
			createOptions |= CreateOptions.FILE_RANDOM_ACCESS;
		}
		if (accessModeOptions.CachedMode == CachedMode.DoNotCacheFile)
		{
			createOptions |= CreateOptions.FILE_NO_INTERMEDIATE_BUFFERING;
		}
		if (accessModeOptions.WriteThroughMode == WriteThroughMode.WriteThrough)
		{
			createOptions |= CreateOptions.FILE_WRITE_THROUGH;
		}
		return createOptions;
	}

	private static OpenResult ToOpenResult(FileStatus fileStatus)
	{
		switch (fileStatus)
		{
		case FileStatus.FILE_SUPERSEDED:
		case FileStatus.FILE_OVERWRITTEN:
			return OpenResult.FileExistedAndWasTruncated;
		case FileStatus.FILE_CREATED:
			return OpenResult.NotExistedAndWasCreated;
		default:
			return OpenResult.FileExistedAndWasOpened;
		}
	}

	private static OpenAndXResponse CreateResponseForNamedPipe(ushort fileID, OpenResult openResult)
	{
		return new OpenAndXResponse
		{
			FID = fileID,
			AccessRights = AccessRights.SMB_DA_ACCESS_READ_WRITE,
			ResourceType = ResourceType.FileTypeMessageModePipe,
			NMPipeStatus = 
			{
				ICount = byte.MaxValue,
				ReadMode = ReadMode.MessageMode,
				NamedPipeType = NamedPipeType.MessageModePipe
			},
			OpenResults = 
			{
				OpenResult = openResult
			}
		};
	}

	private static OpenAndXResponseExtended CreateResponseExtendedForNamedPipe(ushort fileID, OpenResult openResult)
	{
		return new OpenAndXResponseExtended
		{
			FID = fileID,
			AccessRights = AccessRights.SMB_DA_ACCESS_READ_WRITE,
			ResourceType = ResourceType.FileTypeMessageModePipe,
			NMPipeStatus = 
			{
				ICount = byte.MaxValue,
				ReadMode = ReadMode.MessageMode,
				NamedPipeType = NamedPipeType.MessageModePipe
			},
			OpenResults = 
			{
				OpenResult = openResult
			}
		};
	}

	private static OpenAndXResponse CreateResponseFromFileInfo(FileNetworkOpenInformation fileInfo, ushort fileID, OpenResult openResult)
	{
		return new OpenAndXResponse
		{
			FID = fileID,
			FileAttrs = SMB1FileStoreHelper.GetFileAttributes(fileInfo.FileAttributes),
			LastWriteTime = fileInfo.LastWriteTime,
			FileDataSize = (uint)Math.Min(4294967295L, fileInfo.EndOfFile),
			AccessRights = AccessRights.SMB_DA_ACCESS_READ,
			ResourceType = ResourceType.FileTypeDisk,
			OpenResults = 
			{
				OpenResult = openResult
			}
		};
	}

	private static OpenAndXResponseExtended CreateResponseExtendedFromFileInfo(FileNetworkOpenInformation fileInfo, ushort fileID, OpenResult openResult)
	{
		return new OpenAndXResponseExtended
		{
			FID = fileID,
			FileAttrs = SMB1FileStoreHelper.GetFileAttributes(fileInfo.FileAttributes),
			LastWriteTime = fileInfo.LastWriteTime,
			FileDataSize = (uint)Math.Min(4294967295L, fileInfo.EndOfFile),
			AccessRights = AccessRights.SMB_DA_ACCESS_READ,
			ResourceType = ResourceType.FileTypeDisk,
			OpenResults = 
			{
				OpenResult = openResult
			},
			MaximalAccessRights = (AccessMask)2032063u,
			GuestMaximalAccessRights = (AccessMask)1180059u
		};
	}
}
