using System.IO;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public class NTFileStoreHelper
{
	public static FileAccess ToCreateFileAccess(AccessMask desiredAccess, CreateDisposition createDisposition)
	{
		FileAccess fileAccess = (FileAccess)0;
		if ((desiredAccess & (AccessMask)1u) != 0 || (desiredAccess & (AccessMask)8u) != 0 || (desiredAccess & (AccessMask)128u) != 0 || (desiredAccess & AccessMask.MAXIMUM_ALLOWED) != 0 || (desiredAccess & AccessMask.GENERIC_ALL) != 0 || (desiredAccess & AccessMask.GENERIC_READ) != 0)
		{
			fileAccess |= FileAccess.Read;
		}
		if ((desiredAccess & (AccessMask)2u) != 0 || (desiredAccess & (AccessMask)4u) != 0 || (desiredAccess & (AccessMask)16u) != 0 || (desiredAccess & (AccessMask)256u) != 0 || (desiredAccess & AccessMask.DELETE) != 0 || (desiredAccess & AccessMask.WRITE_DAC) != 0 || (desiredAccess & AccessMask.WRITE_OWNER) != 0 || (desiredAccess & AccessMask.MAXIMUM_ALLOWED) != 0 || (desiredAccess & AccessMask.GENERIC_ALL) != 0 || (desiredAccess & AccessMask.GENERIC_WRITE) != 0)
		{
			fileAccess |= FileAccess.Write;
		}
		if ((desiredAccess & (AccessMask)64u) != 0)
		{
			fileAccess |= FileAccess.Write;
		}
		if (createDisposition == CreateDisposition.FILE_CREATE || createDisposition == CreateDisposition.FILE_SUPERSEDE || createDisposition == CreateDisposition.FILE_OPEN_IF || createDisposition == CreateDisposition.FILE_OVERWRITE || createDisposition == CreateDisposition.FILE_OVERWRITE_IF)
		{
			fileAccess |= FileAccess.Write;
		}
		return fileAccess;
	}

	public static FileAccess ToFileAccess(AccessMask desiredAccess)
	{
		return ToFileAccess((FileAccessMask)desiredAccess);
	}

	public static FileAccess ToFileAccess(FileAccessMask desiredAccess)
	{
		FileAccess fileAccess = (FileAccess)0;
		if ((desiredAccess & FileAccessMask.FILE_READ_DATA) != 0 || (desiredAccess & FileAccessMask.MAXIMUM_ALLOWED) != 0 || (desiredAccess & FileAccessMask.GENERIC_ALL) != 0 || (desiredAccess & FileAccessMask.GENERIC_READ) != 0)
		{
			fileAccess |= FileAccess.Read;
		}
		if ((desiredAccess & FileAccessMask.FILE_WRITE_DATA) != 0 || (desiredAccess & FileAccessMask.FILE_APPEND_DATA) != 0 || (desiredAccess & FileAccessMask.MAXIMUM_ALLOWED) != 0 || (desiredAccess & FileAccessMask.GENERIC_ALL) != 0 || (desiredAccess & FileAccessMask.GENERIC_WRITE) != 0)
		{
			fileAccess |= FileAccess.Write;
		}
		return fileAccess;
	}

	public static FileShare ToFileShare(ShareAccess shareAccess)
	{
		FileShare fileShare = FileShare.None;
		if ((shareAccess & ShareAccess.Read) != 0)
		{
			fileShare |= FileShare.Read;
		}
		if ((shareAccess & ShareAccess.Write) != 0)
		{
			fileShare |= FileShare.Write;
		}
		if ((shareAccess & ShareAccess.Delete) != 0)
		{
			fileShare |= FileShare.Delete;
		}
		return fileShare;
	}

	public static FileNetworkOpenInformation GetNetworkOpenInformation(INTFileStore fileStore, string path, SecurityContext securityContext)
	{
		if (fileStore.CreateFile(out var handle, out var _, path, (AccessMask)128u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, (CreateOptions)0u, securityContext) != 0)
		{
			return null;
		}
		FileInformation result;
		NTStatus fileInformation = fileStore.GetFileInformation(out result, handle, FileInformationClass.FileNetworkOpenInformation);
		fileStore.CloseFile(handle);
		if (fileInformation != 0)
		{
			return null;
		}
		return (FileNetworkOpenInformation)result;
	}

	public static FileNetworkOpenInformation GetNetworkOpenInformation(INTFileStore fileStore, object handle)
	{
		if (fileStore.GetFileInformation(out var result, handle, FileInformationClass.FileNetworkOpenInformation) != 0)
		{
			return null;
		}
		return (FileNetworkOpenInformation)result;
	}
}
