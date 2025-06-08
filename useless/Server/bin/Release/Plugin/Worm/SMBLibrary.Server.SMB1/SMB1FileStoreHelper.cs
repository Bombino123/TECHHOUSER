using System;
using System.Collections.Generic;
using SMBLibrary.SMB1;

namespace SMBLibrary.Server.SMB1;

internal class SMB1FileStoreHelper
{
	public static NTStatus CreateDirectory(INTFileStore fileStore, string path, SecurityContext securityContext)
	{
		object handle;
		FileStatus fileStatus;
		NTStatus nTStatus = fileStore.CreateFile(out handle, out fileStatus, path, (AccessMask)4u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_CREATE, CreateOptions.FILE_DIRECTORY_FILE, securityContext);
		if (nTStatus != 0)
		{
			return nTStatus;
		}
		fileStore.CloseFile(handle);
		return nTStatus;
	}

	public static NTStatus DeleteDirectory(INTFileStore fileStore, string path, SecurityContext securityContext)
	{
		return Delete(fileStore, path, CreateOptions.FILE_DIRECTORY_FILE, securityContext);
	}

	public static NTStatus DeleteFile(INTFileStore fileStore, string path, SecurityContext securityContext)
	{
		return Delete(fileStore, path, CreateOptions.FILE_NON_DIRECTORY_FILE, securityContext);
	}

	public static NTStatus Delete(INTFileStore fileStore, string path, CreateOptions createOptions, SecurityContext securityContext)
	{
		ShareAccess shareAccess = ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete;
		NTStatus nTStatus = fileStore.CreateFile(out var handle, out var _, path, AccessMask.DELETE, (FileAttributes)0u, shareAccess, CreateDisposition.FILE_OPEN, createOptions, securityContext);
		if (nTStatus != 0)
		{
			return nTStatus;
		}
		FileDispositionInformation fileDispositionInformation = new FileDispositionInformation();
		fileDispositionInformation.DeletePending = true;
		nTStatus = fileStore.SetFileInformation(handle, fileDispositionInformation);
		fileStore.CloseFile(handle);
		return nTStatus;
	}

	public static NTStatus Rename(INTFileStore fileStore, string oldName, string newName, SMBFileAttributes searchAttributes, SecurityContext securityContext)
	{
		CreateOptions createOptions = (CreateOptions)0u;
		if ((searchAttributes & SMBFileAttributes.Directory) == 0)
		{
			createOptions = CreateOptions.FILE_NON_DIRECTORY_FILE;
		}
		ShareAccess shareAccess = ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete;
		NTStatus nTStatus = fileStore.CreateFile(out var handle, out var _, oldName, AccessMask.DELETE, (FileAttributes)0u, shareAccess, CreateDisposition.FILE_OPEN, createOptions, securityContext);
		if (nTStatus != 0)
		{
			return nTStatus;
		}
		FileRenameInformationType2 fileRenameInformationType = new FileRenameInformationType2();
		fileRenameInformationType.ReplaceIfExists = false;
		fileRenameInformationType.FileName = newName;
		nTStatus = fileStore.SetFileInformation(handle, fileRenameInformationType);
		fileStore.CloseFile(handle);
		return nTStatus;
	}

	public static NTStatus CheckDirectory(INTFileStore fileStore, string path, SecurityContext securityContext)
	{
		object handle;
		FileStatus fileStatus;
		NTStatus nTStatus = fileStore.CreateFile(out handle, out fileStatus, path, (AccessMask)0u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, securityContext);
		if (nTStatus != 0)
		{
			return nTStatus;
		}
		fileStore.CloseFile(handle);
		return NTStatus.STATUS_SUCCESS;
	}

	public static NTStatus QueryInformation(out FileNetworkOpenInformation fileInfo, INTFileStore fileStore, string path, SecurityContext securityContext)
	{
		object handle;
		FileStatus fileStatus;
		NTStatus nTStatus = fileStore.CreateFile(out handle, out fileStatus, path, (AccessMask)128u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, (CreateOptions)0u, securityContext);
		if (nTStatus != 0)
		{
			fileInfo = null;
			return nTStatus;
		}
		fileInfo = NTFileStoreHelper.GetNetworkOpenInformation(fileStore, handle);
		fileStore.CloseFile(handle);
		return NTStatus.STATUS_SUCCESS;
	}

	public static NTStatus SetInformation(INTFileStore fileStore, string path, SMBFileAttributes fileAttributes, DateTime? lastWriteTime, SecurityContext securityContext)
	{
		NTStatus nTStatus = fileStore.CreateFile(out var handle, out var _, path, (AccessMask)256u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, (CreateOptions)0u, securityContext);
		if (nTStatus != 0)
		{
			return nTStatus;
		}
		FileBasicInformation fileBasicInformation = new FileBasicInformation();
		fileBasicInformation.LastWriteTime = lastWriteTime;
		if ((int)(fileAttributes & SMBFileAttributes.Hidden) > 0)
		{
			fileBasicInformation.FileAttributes |= FileAttributes.Hidden;
		}
		if ((int)(fileAttributes & SMBFileAttributes.ReadOnly) > 0)
		{
			fileBasicInformation.FileAttributes |= FileAttributes.ReadOnly;
		}
		if ((int)(fileAttributes & SMBFileAttributes.Archive) > 0)
		{
			fileBasicInformation.FileAttributes |= FileAttributes.Archive;
		}
		nTStatus = fileStore.SetFileInformation(handle, fileBasicInformation);
		fileStore.CloseFile(handle);
		return nTStatus;
	}

	public static NTStatus SetInformation2(INTFileStore fileStore, object handle, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
	{
		FileNetworkOpenInformation networkOpenInformation = NTFileStoreHelper.GetNetworkOpenInformation(fileStore, handle);
		FileBasicInformation fileBasicInformation = new FileBasicInformation();
		fileBasicInformation.FileAttributes = networkOpenInformation.FileAttributes;
		fileBasicInformation.CreationTime = creationTime;
		fileBasicInformation.LastAccessTime = lastAccessTime;
		fileBasicInformation.LastWriteTime = lastWriteTime;
		return fileStore.SetFileInformation(handle, fileBasicInformation);
	}

	public static SMBFileAttributes GetFileAttributes(FileAttributes attributes)
	{
		SMBFileAttributes sMBFileAttributes = SMBFileAttributes.Normal;
		if ((attributes & FileAttributes.Hidden) != 0)
		{
			sMBFileAttributes |= SMBFileAttributes.Hidden;
		}
		if ((attributes & FileAttributes.ReadOnly) != 0)
		{
			sMBFileAttributes |= SMBFileAttributes.ReadOnly;
		}
		if ((attributes & FileAttributes.Archive) != 0)
		{
			sMBFileAttributes |= SMBFileAttributes.Archive;
		}
		if ((attributes & FileAttributes.Directory) != 0)
		{
			sMBFileAttributes |= SMBFileAttributes.Directory;
		}
		return sMBFileAttributes;
	}

	public static NTStatus GetFileInformation(out QueryInformation result, INTFileStore fileStore, string path, QueryInformationLevel informationLevel, SecurityContext securityContext)
	{
		object handle;
		FileStatus fileStatus;
		NTStatus nTStatus = fileStore.CreateFile(out handle, out fileStatus, path, (AccessMask)128u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, (CreateOptions)0u, securityContext);
		if (nTStatus != 0)
		{
			result = null;
			return nTStatus;
		}
		NTStatus fileInformation = GetFileInformation(out result, fileStore, handle, informationLevel);
		fileStore.CloseFile(handle);
		return fileInformation;
	}

	public static NTStatus GetFileInformation(out FileInformation result, INTFileStore fileStore, string path, FileInformationClass informationClass, SecurityContext securityContext)
	{
		object handle;
		FileStatus fileStatus;
		NTStatus nTStatus = fileStore.CreateFile(out handle, out fileStatus, path, (AccessMask)128u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, (CreateOptions)0u, securityContext);
		if (nTStatus != 0)
		{
			result = null;
			return nTStatus;
		}
		NTStatus fileInformation = fileStore.GetFileInformation(out result, handle, informationClass);
		fileStore.CloseFile(handle);
		return fileInformation;
	}

	public static NTStatus GetFileInformation(out QueryInformation result, INTFileStore fileStore, object handle, QueryInformationLevel informationLevel)
	{
		result = null;
		FileInformationClass informationClass;
		try
		{
			informationClass = QueryInformationHelper.ToFileInformationClass(informationLevel);
		}
		catch (UnsupportedInformationLevelException)
		{
			return NTStatus.STATUS_OS2_INVALID_LEVEL;
		}
		FileInformation result2;
		NTStatus fileInformation = fileStore.GetFileInformation(out result2, handle, informationClass);
		if (fileInformation != 0)
		{
			return fileInformation;
		}
		result = QueryInformationHelper.FromFileInformation(result2);
		return NTStatus.STATUS_SUCCESS;
	}

	public static NTStatus QueryDirectory(out List<QueryDirectoryFileInformation> result, INTFileStore fileStore, string fileNamePattern, FileInformationClass fileInformation, SecurityContext securityContext)
	{
		int num = fileNamePattern.LastIndexOf('\\');
		if (num >= 0)
		{
			string path = fileNamePattern.Substring(0, num + 1);
			string fileName = fileNamePattern.Substring(num + 1);
			DirectoryAccessMask desiredAccess = DirectoryAccessMask.FILE_LIST_DIRECTORY | DirectoryAccessMask.FILE_TRAVERSE | DirectoryAccessMask.SYNCHRONIZE;
			CreateOptions createOptions = CreateOptions.FILE_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT;
			NTStatus nTStatus = fileStore.CreateFile(out var handle, out var _, path, (AccessMask)desiredAccess, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, createOptions, securityContext);
			if (nTStatus != 0)
			{
				result = null;
				return nTStatus;
			}
			nTStatus = fileStore.QueryDirectory(out result, handle, fileName, fileInformation);
			fileStore.CloseFile(handle);
			return nTStatus;
		}
		result = null;
		return NTStatus.STATUS_INVALID_PARAMETER;
	}

	public static NTStatus GetFileSystemInformation(out QueryFSInformation result, INTFileStore fileStore, QueryFSInformationLevel informationLevel)
	{
		result = null;
		FileSystemInformationClass informationClass;
		try
		{
			informationClass = QueryFSInformationHelper.ToFileSystemInformationClass(informationLevel);
		}
		catch (UnsupportedInformationLevelException)
		{
			return NTStatus.STATUS_OS2_INVALID_LEVEL;
		}
		FileSystemInformation result2;
		NTStatus fileSystemInformation = fileStore.GetFileSystemInformation(out result2, informationClass);
		if (fileSystemInformation != 0)
		{
			return fileSystemInformation;
		}
		result = QueryFSInformationHelper.FromFileSystemInformation(result2);
		return NTStatus.STATUS_SUCCESS;
	}

	public static NTStatus SetFileInformation(INTFileStore fileStore, object handle, SetInformation information)
	{
		FileInformation information2 = SetInformationHelper.ToFileInformation(information);
		return fileStore.SetFileInformation(handle, information2);
	}
}
