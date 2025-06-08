using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public interface INTFileStore
{
	NTStatus CreateFile(out object handle, out FileStatus fileStatus, string path, AccessMask desiredAccess, FileAttributes fileAttributes, ShareAccess shareAccess, CreateDisposition createDisposition, CreateOptions createOptions, SecurityContext securityContext);

	NTStatus CloseFile(object handle);

	NTStatus ReadFile(out byte[] data, object handle, long offset, int maxCount);

	NTStatus WriteFile(out int numberOfBytesWritten, object handle, long offset, byte[] data);

	NTStatus FlushFileBuffers(object handle);

	NTStatus LockFile(object handle, long byteOffset, long length, bool exclusiveLock);

	NTStatus UnlockFile(object handle, long byteOffset, long length);

	NTStatus QueryDirectory(out List<QueryDirectoryFileInformation> result, object handle, string fileName, FileInformationClass informationClass);

	NTStatus GetFileInformation(out FileInformation result, object handle, FileInformationClass informationClass);

	NTStatus SetFileInformation(object handle, FileInformation information);

	NTStatus GetFileSystemInformation(out FileSystemInformation result, FileSystemInformationClass informationClass);

	NTStatus SetFileSystemInformation(FileSystemInformation information);

	NTStatus GetSecurityInformation(out SecurityDescriptor result, object handle, SecurityInformation securityInformation);

	NTStatus SetSecurityInformation(object handle, SecurityInformation securityInformation, SecurityDescriptor securityDescriptor);

	NTStatus NotifyChange(out object ioRequest, object handle, NotifyChangeFilter completionFilter, bool watchTree, int outputBufferSize, OnNotifyChangeCompleted onNotifyChangeCompleted, object context);

	NTStatus Cancel(object ioRequest);

	NTStatus DeviceIOControl(object handle, uint ctlCode, byte[] input, out byte[] output, int maxOutputLength);
}
