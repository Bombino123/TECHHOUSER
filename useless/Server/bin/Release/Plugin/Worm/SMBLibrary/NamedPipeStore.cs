using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SMBLibrary.Services;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class NamedPipeStore : INTFileStore
{
	private List<RemoteService> m_services;

	public NamedPipeStore(List<RemoteService> services)
	{
		m_services = services;
	}

	public NTStatus CreateFile(out object handle, out FileStatus fileStatus, string path, AccessMask desiredAccess, FileAttributes fileAttributes, ShareAccess shareAccess, CreateDisposition createDisposition, CreateOptions createOptions, SecurityContext securityContext)
	{
		fileStatus = FileStatus.FILE_DOES_NOT_EXIST;
		RemoteService service = GetService(path);
		if (service != null)
		{
			RPCPipeStream stream = new RPCPipeStream(service);
			handle = new FileHandle(path, isDirectory: false, stream, deleteOnClose: false);
			fileStatus = FileStatus.FILE_OPENED;
			return NTStatus.STATUS_SUCCESS;
		}
		handle = null;
		return NTStatus.STATUS_OBJECT_PATH_NOT_FOUND;
	}

	public NTStatus CloseFile(object handle)
	{
		FileHandle fileHandle = (FileHandle)handle;
		if (fileHandle.Stream != null)
		{
			fileHandle.Stream.Close();
		}
		return NTStatus.STATUS_SUCCESS;
	}

	private RemoteService GetService(string path)
	{
		if (path.StartsWith("\\"))
		{
			path = path.Substring(1);
		}
		foreach (RemoteService service in m_services)
		{
			if (string.Equals(path, service.PipeName, StringComparison.OrdinalIgnoreCase))
			{
				return service;
			}
		}
		return null;
	}

	public NTStatus ReadFile(out byte[] data, object handle, long offset, int maxCount)
	{
		Stream stream = ((FileHandle)handle).Stream;
		data = new byte[maxCount];
		int num = stream.Read(data, 0, maxCount);
		if (num < maxCount)
		{
			data = ByteReader.ReadBytes(data, 0, num);
		}
		return NTStatus.STATUS_SUCCESS;
	}

	public NTStatus WriteFile(out int numberOfBytesWritten, object handle, long offset, byte[] data)
	{
		((FileHandle)handle).Stream.Write(data, 0, data.Length);
		numberOfBytesWritten = data.Length;
		return NTStatus.STATUS_SUCCESS;
	}

	public NTStatus FlushFileBuffers(object handle)
	{
		FileHandle fileHandle = (FileHandle)handle;
		if (fileHandle.Stream != null)
		{
			fileHandle.Stream.Flush();
		}
		return NTStatus.STATUS_SUCCESS;
	}

	public NTStatus LockFile(object handle, long byteOffset, long length, bool exclusiveLock)
	{
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus UnlockFile(object handle, long byteOffset, long length)
	{
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus DeviceIOControl(object handle, uint ctlCode, byte[] input, out byte[] output, int maxOutputLength)
	{
		output = null;
		switch (ctlCode)
		{
		case 1114136u:
		{
			PipeWaitRequest pipeWaitRequest;
			try
			{
				pipeWaitRequest = new PipeWaitRequest(input, 0);
			}
			catch
			{
				return NTStatus.STATUS_INVALID_PARAMETER;
			}
			if (GetService(pipeWaitRequest.Name) == null)
			{
				return NTStatus.STATUS_OBJECT_NAME_NOT_FOUND;
			}
			output = new byte[0];
			return NTStatus.STATUS_SUCCESS;
		}
		case 1163287u:
		{
			int numberOfBytesWritten;
			NTStatus nTStatus = WriteFile(out numberOfBytesWritten, handle, 0L, input);
			if (nTStatus != 0)
			{
				return nTStatus;
			}
			int messageLength = ((RPCPipeStream)((FileHandle)handle).Stream).MessageLength;
			NTStatus nTStatus2 = ReadFile(out output, handle, 0L, maxOutputLength);
			if (nTStatus2 != 0)
			{
				return nTStatus2;
			}
			if (output.Length < messageLength)
			{
				return NTStatus.STATUS_BUFFER_OVERFLOW;
			}
			return NTStatus.STATUS_SUCCESS;
		}
		default:
			return NTStatus.STATUS_NOT_SUPPORTED;
		}
	}

	public NTStatus QueryDirectory(out List<QueryDirectoryFileInformation> result, object directoryHandle, string fileName, FileInformationClass informationClass)
	{
		result = null;
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus GetFileInformation(out FileInformation result, object handle, FileInformationClass informationClass)
	{
		switch (informationClass)
		{
		case FileInformationClass.FileBasicInformation:
		{
			FileBasicInformation fileBasicInformation = new FileBasicInformation();
			fileBasicInformation.FileAttributes = FileAttributes.Temporary;
			result = fileBasicInformation;
			return NTStatus.STATUS_SUCCESS;
		}
		case FileInformationClass.FileStandardInformation:
		{
			FileStandardInformation fileStandardInformation = new FileStandardInformation();
			fileStandardInformation.DeletePending = false;
			result = fileStandardInformation;
			return NTStatus.STATUS_SUCCESS;
		}
		case FileInformationClass.FileNetworkOpenInformation:
		{
			FileNetworkOpenInformation fileNetworkOpenInformation = new FileNetworkOpenInformation();
			fileNetworkOpenInformation.FileAttributes = FileAttributes.Temporary;
			result = fileNetworkOpenInformation;
			return NTStatus.STATUS_SUCCESS;
		}
		default:
			result = null;
			return NTStatus.STATUS_INVALID_INFO_CLASS;
		}
	}

	public NTStatus SetFileInformation(object handle, FileInformation information)
	{
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus GetFileSystemInformation(out FileSystemInformation result, FileSystemInformationClass informationClass)
	{
		result = null;
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus SetFileSystemInformation(FileSystemInformation information)
	{
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus GetSecurityInformation(out SecurityDescriptor result, object handle, SecurityInformation securityInformation)
	{
		result = null;
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus SetSecurityInformation(object handle, SecurityInformation securityInformation, SecurityDescriptor securityDescriptor)
	{
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus NotifyChange(out object ioRequest, object handle, NotifyChangeFilter completionFilter, bool watchTree, int outputBufferSize, OnNotifyChangeCompleted onNotifyChangeCompleted, object context)
	{
		ioRequest = null;
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus Cancel(object ioRequest)
	{
		return NTStatus.STATUS_NOT_SUPPORTED;
	}
}
