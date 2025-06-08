using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SMBLibrary.SMB2;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class SMB2FileStore : ISMBFileStore, INTFileStore
{
	private const int BytesPerCredit = 65536;

	private SMB2Client m_client;

	private uint m_treeID;

	private bool m_encryptShareData;

	public uint MaxReadSize => m_client.MaxReadSize;

	public uint MaxWriteSize => m_client.MaxWriteSize;

	public SMB2FileStore(SMB2Client client, uint treeID, bool encryptShareData)
	{
		m_client = client;
		m_treeID = treeID;
		m_encryptShareData = encryptShareData;
	}

	public NTStatus CreateFile(out object handle, out FileStatus fileStatus, string path, AccessMask desiredAccess, FileAttributes fileAttributes, ShareAccess shareAccess, CreateDisposition createDisposition, CreateOptions createOptions, SecurityContext securityContext)
	{
		handle = null;
		fileStatus = FileStatus.FILE_DOES_NOT_EXIST;
		CreateRequest createRequest = new CreateRequest();
		createRequest.Name = path;
		createRequest.DesiredAccess = desiredAccess;
		createRequest.FileAttributes = fileAttributes;
		createRequest.ShareAccess = shareAccess;
		createRequest.CreateDisposition = createDisposition;
		createRequest.CreateOptions = createOptions;
		createRequest.ImpersonationLevel = ImpersonationLevel.Impersonation;
		TrySendCommand(createRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(createRequest.MessageID);
		if (sMB2Command != null)
		{
			if (sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is CreateResponse)
			{
				CreateResponse createResponse = (CreateResponse)sMB2Command;
				handle = createResponse.FileId;
				fileStatus = ToFileStatus(createResponse.CreateAction);
			}
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus CloseFile(object handle)
	{
		CloseRequest closeRequest = new CloseRequest();
		closeRequest.FileId = (FileID)handle;
		TrySendCommand(closeRequest);
		return m_client.WaitForCommand(closeRequest.MessageID)?.Header.Status ?? NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus ReadFile(out byte[] data, object handle, long offset, int maxCount)
	{
		data = null;
		ReadRequest readRequest = new ReadRequest();
		readRequest.Header.CreditCharge = (ushort)Math.Ceiling((double)maxCount / 65536.0);
		readRequest.FileId = (FileID)handle;
		readRequest.Offset = (ulong)offset;
		readRequest.ReadLength = (uint)maxCount;
		TrySendCommand(readRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(readRequest.MessageID);
		if (sMB2Command != null)
		{
			if (sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is ReadResponse)
			{
				data = ((ReadResponse)sMB2Command).Data;
			}
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus WriteFile(out int numberOfBytesWritten, object handle, long offset, byte[] data)
	{
		numberOfBytesWritten = 0;
		WriteRequest writeRequest = new WriteRequest();
		writeRequest.Header.CreditCharge = (ushort)Math.Ceiling((double)data.Length / 65536.0);
		writeRequest.FileId = (FileID)handle;
		writeRequest.Offset = (ulong)offset;
		writeRequest.Data = data;
		TrySendCommand(writeRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(writeRequest.MessageID);
		if (sMB2Command != null)
		{
			if (sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is WriteResponse)
			{
				numberOfBytesWritten = (int)((WriteResponse)sMB2Command).Count;
			}
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus FlushFileBuffers(object handle)
	{
		FlushRequest flushRequest = new FlushRequest();
		flushRequest.FileId = (FileID)handle;
		TrySendCommand(flushRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(flushRequest.MessageID);
		if (sMB2Command != null && sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is FlushResponse)
		{
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus LockFile(object handle, long byteOffset, long length, bool exclusiveLock)
	{
		throw new NotImplementedException();
	}

	public NTStatus UnlockFile(object handle, long byteOffset, long length)
	{
		throw new NotImplementedException();
	}

	public NTStatus QueryDirectory(out List<QueryDirectoryFileInformation> result, object handle, string fileName, FileInformationClass informationClass)
	{
		result = new List<QueryDirectoryFileInformation>();
		QueryDirectoryRequest queryDirectoryRequest = new QueryDirectoryRequest();
		queryDirectoryRequest.Header.CreditCharge = (ushort)Math.Ceiling((double)m_client.MaxTransactSize / 65536.0);
		queryDirectoryRequest.FileInformationClass = informationClass;
		queryDirectoryRequest.Reopen = true;
		queryDirectoryRequest.FileId = (FileID)handle;
		queryDirectoryRequest.OutputBufferLength = m_client.MaxTransactSize;
		queryDirectoryRequest.FileName = fileName;
		TrySendCommand(queryDirectoryRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(queryDirectoryRequest.MessageID);
		if (sMB2Command != null)
		{
			while (sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is QueryDirectoryResponse)
			{
				List<QueryDirectoryFileInformation> fileInformationList = ((QueryDirectoryResponse)sMB2Command).GetFileInformationList(informationClass);
				result.AddRange(fileInformationList);
				queryDirectoryRequest.Reopen = false;
				TrySendCommand(queryDirectoryRequest);
				sMB2Command = m_client.WaitForCommand(queryDirectoryRequest.MessageID);
			}
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus GetFileInformation(out FileInformation result, object handle, FileInformationClass informationClass)
	{
		result = null;
		QueryInfoRequest queryInfoRequest = new QueryInfoRequest();
		queryInfoRequest.InfoType = InfoType.File;
		queryInfoRequest.FileInformationClass = informationClass;
		queryInfoRequest.OutputBufferLength = 4096u;
		queryInfoRequest.FileId = (FileID)handle;
		TrySendCommand(queryInfoRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(queryInfoRequest.MessageID);
		if (sMB2Command != null)
		{
			if (sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is QueryInfoResponse)
			{
				result = ((QueryInfoResponse)sMB2Command).GetFileInformation(informationClass);
			}
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus SetFileInformation(object handle, FileInformation information)
	{
		SetInfoRequest setInfoRequest = new SetInfoRequest();
		setInfoRequest.InfoType = InfoType.File;
		setInfoRequest.FileInformationClass = information.FileInformationClass;
		setInfoRequest.FileId = (FileID)handle;
		setInfoRequest.SetFileInformation(information);
		TrySendCommand(setInfoRequest);
		return m_client.WaitForCommand(setInfoRequest.MessageID)?.Header.Status ?? NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus GetFileSystemInformation(out FileSystemInformation result, FileSystemInformationClass informationClass)
	{
		result = null;
		NTStatus nTStatus = CreateFile(out var handle, out var _, string.Empty, (AccessMask)1048705u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT, null);
		if (nTStatus != 0)
		{
			return nTStatus;
		}
		nTStatus = GetFileSystemInformation(out result, handle, informationClass);
		CloseFile(handle);
		return nTStatus;
	}

	public NTStatus GetFileSystemInformation(out FileSystemInformation result, object handle, FileSystemInformationClass informationClass)
	{
		result = null;
		QueryInfoRequest queryInfoRequest = new QueryInfoRequest();
		queryInfoRequest.InfoType = InfoType.FileSystem;
		queryInfoRequest.FileSystemInformationClass = informationClass;
		queryInfoRequest.OutputBufferLength = 4096u;
		queryInfoRequest.FileId = (FileID)handle;
		TrySendCommand(queryInfoRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(queryInfoRequest.MessageID);
		if (sMB2Command != null)
		{
			if (sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is QueryInfoResponse)
			{
				result = ((QueryInfoResponse)sMB2Command).GetFileSystemInformation(informationClass);
			}
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus SetFileSystemInformation(FileSystemInformation information)
	{
		throw new NotImplementedException();
	}

	public NTStatus GetSecurityInformation(out SecurityDescriptor result, object handle, SecurityInformation securityInformation)
	{
		result = null;
		QueryInfoRequest queryInfoRequest = new QueryInfoRequest();
		queryInfoRequest.InfoType = InfoType.Security;
		queryInfoRequest.SecurityInformation = securityInformation;
		queryInfoRequest.OutputBufferLength = 4096u;
		queryInfoRequest.FileId = (FileID)handle;
		TrySendCommand(queryInfoRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(queryInfoRequest.MessageID);
		if (sMB2Command != null)
		{
			if (sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is QueryInfoResponse)
			{
				result = ((QueryInfoResponse)sMB2Command).GetSecurityInformation();
			}
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus SetSecurityInformation(object handle, SecurityInformation securityInformation, SecurityDescriptor securityDescriptor)
	{
		return NTStatus.STATUS_NOT_SUPPORTED;
	}

	public NTStatus NotifyChange(out object ioRequest, object handle, NotifyChangeFilter completionFilter, bool watchTree, int outputBufferSize, OnNotifyChangeCompleted onNotifyChangeCompleted, object context)
	{
		throw new NotImplementedException();
	}

	public NTStatus Cancel(object ioRequest)
	{
		throw new NotImplementedException();
	}

	public NTStatus DeviceIOControl(object handle, uint ctlCode, byte[] input, out byte[] output, int maxOutputLength)
	{
		output = null;
		IOCtlRequest iOCtlRequest = new IOCtlRequest();
		iOCtlRequest.Header.CreditCharge = (ushort)Math.Ceiling((double)maxOutputLength / 65536.0);
		iOCtlRequest.CtlCode = ctlCode;
		iOCtlRequest.IsFSCtl = true;
		iOCtlRequest.FileId = (FileID)handle;
		iOCtlRequest.Input = input;
		iOCtlRequest.MaxOutputResponse = (uint)maxOutputLength;
		TrySendCommand(iOCtlRequest);
		SMB2Command sMB2Command = m_client.WaitForCommand(iOCtlRequest.MessageID);
		if (sMB2Command != null)
		{
			if ((sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS || sMB2Command.Header.Status == NTStatus.STATUS_BUFFER_OVERFLOW) && sMB2Command is IOCtlResponse)
			{
				output = ((IOCtlResponse)sMB2Command).Output;
			}
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus Disconnect()
	{
		TreeDisconnectRequest treeDisconnectRequest = new TreeDisconnectRequest();
		TrySendCommand(treeDisconnectRequest);
		return m_client.WaitForCommand(treeDisconnectRequest.MessageID)?.Header.Status ?? NTStatus.STATUS_INVALID_SMB;
	}

	private void TrySendCommand(SMB2Command request)
	{
		request.Header.TreeID = m_treeID;
		if (!m_client.IsConnected)
		{
			throw new InvalidOperationException("The client is no longer connected");
		}
		m_client.TrySendCommand(request, m_encryptShareData);
	}

	private static FileStatus ToFileStatus(CreateAction createAction)
	{
		return createAction switch
		{
			CreateAction.FILE_SUPERSEDED => FileStatus.FILE_SUPERSEDED, 
			CreateAction.FILE_OPENED => FileStatus.FILE_OPENED, 
			CreateAction.FILE_CREATED => FileStatus.FILE_CREATED, 
			CreateAction.FILE_OVERWRITTEN => FileStatus.FILE_OVERWRITTEN, 
			_ => FileStatus.FILE_OPENED, 
		};
	}
}
