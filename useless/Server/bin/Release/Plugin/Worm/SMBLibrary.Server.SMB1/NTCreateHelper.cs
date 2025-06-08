using System.IO;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class NTCreateHelper
{
	internal static SMB1Command GetNTCreateResponse(SMB1Header header, NTCreateAndXRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		bool flag = (request.Flags & NTCreateFlags.NT_CREATE_REQUEST_EXTENDED_RESPONSE) != 0;
		string text = request.FileName;
		if (!text.StartsWith("\\"))
		{
			text = "\\" + text;
		}
		FileAccess requestedAccess = NTFileStoreHelper.ToCreateFileAccess(request.DesiredAccess, request.CreateDisposition);
		if (share is FileSystemShare && !((FileSystemShare)share).HasAccess(session.SecurityContext, text, requestedAccess))
		{
			state.LogToServer(Severity.Verbose, "Create: Opening '{0}{1}' failed. User '{2}' was denied access.", share.Name, request.FileName, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		FileAttributes fileAttributes = ToFileAttributes(request.ExtFileAttributes);
		AccessMask desiredAccess = request.DesiredAccess | (AccessMask)128u;
		object handle;
		FileStatus fileStatus;
		NTStatus nTStatus = share.FileStore.CreateFile(out handle, out fileStatus, text, desiredAccess, fileAttributes, request.ShareAccess, request.CreateDisposition, request.CreateOptions, session.SecurityContext);
		if (nTStatus != 0)
		{
			state.LogToServer(Severity.Verbose, "Create: Opening '{0}{1}' failed. NTStatus: {2}.", share.Name, text, nTStatus);
			header.Status = nTStatus;
			return new ErrorResponse(request.CommandName);
		}
		FileAccess fileAccess = NTFileStoreHelper.ToFileAccess(desiredAccess);
		ushort? num = session.AddOpenFile(header.TID, share.Name, text, handle, fileAccess);
		if (!num.HasValue)
		{
			share.FileStore.CloseFile(handle);
			state.LogToServer(Severity.Verbose, "Create: Opening '{0}{1}' failed. Too many open files.", share.Name, text);
			header.Status = NTStatus.STATUS_TOO_MANY_OPENED_FILES;
			return new ErrorResponse(request.CommandName);
		}
		string text2 = fileAccess.ToString().Replace(", ", "|");
		string text3 = request.ShareAccess.ToString().Replace(", ", "|");
		state.LogToServer(Severity.Verbose, "Create: Opened '{0}{1}', FileAccess: {2}, ShareAccess: {3}. (UID: {4}, TID: {5}, FID: {6})", share.Name, text, text2, text3, header.UID, header.TID, num.Value);
		if (share is NamedPipeShare)
		{
			if (flag)
			{
				return CreateResponseExtendedForNamedPipe(num.Value, FileStatus.FILE_OPENED);
			}
			return CreateResponseForNamedPipe(num.Value, FileStatus.FILE_OPENED);
		}
		FileNetworkOpenInformation networkOpenInformation = NTFileStoreHelper.GetNetworkOpenInformation(share.FileStore, handle);
		if (flag)
		{
			return CreateResponseExtendedFromFileInformation(networkOpenInformation, num.Value, fileStatus);
		}
		return CreateResponseFromFileInformation(networkOpenInformation, num.Value, fileStatus);
	}

	private static NTCreateAndXResponse CreateResponseForNamedPipe(ushort fileID, FileStatus fileStatus)
	{
		return new NTCreateAndXResponse
		{
			FID = fileID,
			CreateDisposition = ToCreateDisposition(fileStatus),
			ExtFileAttributes = ExtendedFileAttributes.Normal,
			ResourceType = ResourceType.FileTypeMessageModePipe,
			NMPipeStatus = 
			{
				ICount = byte.MaxValue,
				ReadMode = ReadMode.MessageMode,
				NamedPipeType = NamedPipeType.MessageModePipe
			}
		};
	}

	private static NTCreateAndXResponseExtended CreateResponseExtendedForNamedPipe(ushort fileID, FileStatus fileStatus)
	{
		return new NTCreateAndXResponseExtended
		{
			FID = fileID,
			CreateDisposition = ToCreateDisposition(fileStatus),
			ExtFileAttributes = ExtendedFileAttributes.Normal,
			ResourceType = ResourceType.FileTypeMessageModePipe,
			NMPipeStatus = new NamedPipeStatus
			{
				ICount = byte.MaxValue,
				ReadMode = ReadMode.MessageMode,
				NamedPipeType = NamedPipeType.MessageModePipe
			},
			MaximalAccessRights = (AccessMask)2032063u,
			GuestMaximalAccessRights = (AccessMask)1180059u
		};
	}

	private static NTCreateAndXResponse CreateResponseFromFileInformation(FileNetworkOpenInformation fileInfo, ushort fileID, FileStatus fileStatus)
	{
		return new NTCreateAndXResponse
		{
			FID = fileID,
			CreateDisposition = ToCreateDisposition(fileStatus),
			CreateTime = fileInfo.CreationTime,
			LastAccessTime = fileInfo.LastAccessTime,
			LastWriteTime = fileInfo.LastWriteTime,
			LastChangeTime = fileInfo.LastWriteTime,
			AllocationSize = fileInfo.AllocationSize,
			EndOfFile = fileInfo.EndOfFile,
			ExtFileAttributes = (ExtendedFileAttributes)fileInfo.FileAttributes,
			ResourceType = ResourceType.FileTypeDisk,
			Directory = fileInfo.IsDirectory
		};
	}

	private static NTCreateAndXResponseExtended CreateResponseExtendedFromFileInformation(FileNetworkOpenInformation fileInfo, ushort fileID, FileStatus fileStatus)
	{
		return new NTCreateAndXResponseExtended
		{
			FID = fileID,
			CreateDisposition = ToCreateDisposition(fileStatus),
			CreateTime = fileInfo.CreationTime,
			LastAccessTime = fileInfo.LastAccessTime,
			LastWriteTime = fileInfo.LastWriteTime,
			LastChangeTime = fileInfo.LastWriteTime,
			ExtFileAttributes = (ExtendedFileAttributes)fileInfo.FileAttributes,
			AllocationSize = fileInfo.AllocationSize,
			EndOfFile = fileInfo.EndOfFile,
			ResourceType = ResourceType.FileTypeDisk,
			FileStatusFlags = (FileStatusFlags.NO_EAS | FileStatusFlags.NO_SUBSTREAMS | FileStatusFlags.NO_REPARSETAG),
			Directory = fileInfo.IsDirectory,
			MaximalAccessRights = (AccessMask)2032063u,
			GuestMaximalAccessRights = (AccessMask)1180059u
		};
	}

	private static CreateDisposition ToCreateDisposition(FileStatus fileStatus)
	{
		return fileStatus switch
		{
			FileStatus.FILE_SUPERSEDED => CreateDisposition.FILE_SUPERSEDE, 
			FileStatus.FILE_CREATED => CreateDisposition.FILE_CREATE, 
			FileStatus.FILE_OVERWRITTEN => CreateDisposition.FILE_OVERWRITE, 
			_ => CreateDisposition.FILE_OPEN, 
		};
	}

	private static FileAttributes ToFileAttributes(ExtendedFileAttributes extendedFileAttributes)
	{
		return (FileAttributes)((ExtendedFileAttributes.ReadOnly | ExtendedFileAttributes.Hidden | ExtendedFileAttributes.System | ExtendedFileAttributes.Archive | ExtendedFileAttributes.Normal | ExtendedFileAttributes.Temporary | ExtendedFileAttributes.Offline | ExtendedFileAttributes.Encrypted) & extendedFileAttributes);
	}
}
