using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SMBLibrary.SMB1;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class SMB1FileStore : ISMBFileStore, INTFileStore
{
	private SMB1Client m_client;

	private ushort m_treeID;

	public uint MaxReadSize => m_client.MaxReadSize;

	public uint MaxWriteSize => m_client.MaxWriteSize;

	public SMB1FileStore(SMB1Client client, ushort treeID)
	{
		m_client = client;
		m_treeID = treeID;
	}

	public NTStatus CreateFile(out object handle, out FileStatus fileStatus, string path, AccessMask desiredAccess, FileAttributes fileAttributes, ShareAccess shareAccess, CreateDisposition createDisposition, CreateOptions createOptions, SecurityContext securityContext)
	{
		handle = null;
		fileStatus = FileStatus.FILE_DOES_NOT_EXIST;
		NTCreateAndXRequest nTCreateAndXRequest = new NTCreateAndXRequest();
		nTCreateAndXRequest.FileName = path;
		nTCreateAndXRequest.DesiredAccess = desiredAccess;
		nTCreateAndXRequest.ExtFileAttributes = ToExtendedFileAttributes(fileAttributes);
		nTCreateAndXRequest.ShareAccess = shareAccess;
		nTCreateAndXRequest.CreateDisposition = createDisposition;
		nTCreateAndXRequest.CreateOptions = createOptions;
		nTCreateAndXRequest.ImpersonationLevel = ImpersonationLevel.Impersonation;
		TrySendMessage(nTCreateAndXRequest);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_NT_CREATE_ANDX);
		if (sMB1Message != null)
		{
			if (sMB1Message.Commands[0] is NTCreateAndXResponse)
			{
				NTCreateAndXResponse nTCreateAndXResponse = sMB1Message.Commands[0] as NTCreateAndXResponse;
				handle = nTCreateAndXResponse.FID;
				fileStatus = ToFileStatus(nTCreateAndXResponse.CreateDisposition);
				return sMB1Message.Header.Status;
			}
			if (sMB1Message.Commands[0] is ErrorResponse)
			{
				return sMB1Message.Header.Status;
			}
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus CloseFile(object handle)
	{
		CloseRequest closeRequest = new CloseRequest();
		closeRequest.FID = (ushort)handle;
		TrySendMessage(closeRequest);
		return m_client.WaitForMessage(CommandName.SMB_COM_CLOSE)?.Header.Status ?? NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus ReadFile(out byte[] data, object handle, long offset, int maxCount)
	{
		data = null;
		ReadAndXRequest readAndXRequest = new ReadAndXRequest();
		readAndXRequest.FID = (ushort)handle;
		readAndXRequest.Offset = (ulong)offset;
		readAndXRequest.MaxCountLarge = (uint)maxCount;
		TrySendMessage(readAndXRequest);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_READ_ANDX);
		if (sMB1Message != null)
		{
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is ReadAndXResponse)
			{
				data = ((ReadAndXResponse)sMB1Message.Commands[0]).Data;
			}
			return sMB1Message.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus WriteFile(out int numberOfBytesWritten, object handle, long offset, byte[] data)
	{
		numberOfBytesWritten = 0;
		WriteAndXRequest writeAndXRequest = new WriteAndXRequest();
		writeAndXRequest.FID = (ushort)handle;
		writeAndXRequest.Offset = (ulong)offset;
		writeAndXRequest.Data = data;
		TrySendMessage(writeAndXRequest);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_WRITE_ANDX);
		if (sMB1Message != null)
		{
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is WriteAndXResponse)
			{
				numberOfBytesWritten = (int)((WriteAndXResponse)sMB1Message.Commands[0]).Count;
			}
			return sMB1Message.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus FlushFileBuffers(object handle)
	{
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public NTStatus QueryDirectory(out List<FindInformation> result, string fileName, FindInformationLevel informationLevel)
	{
		result = null;
		int num = 4096;
		Transaction2FindFirst2Request transaction2FindFirst2Request = new Transaction2FindFirst2Request();
		transaction2FindFirst2Request.SearchAttributes = SMBFileAttributes.Hidden | SMBFileAttributes.System | SMBFileAttributes.Directory;
		transaction2FindFirst2Request.SearchCount = ushort.MaxValue;
		transaction2FindFirst2Request.Flags = FindFlags.SMB_FIND_CLOSE_AT_EOS;
		transaction2FindFirst2Request.InformationLevel = informationLevel;
		transaction2FindFirst2Request.FileName = fileName;
		Transaction2Request transaction2Request = new Transaction2Request();
		transaction2Request.Setup = transaction2FindFirst2Request.GetSetup();
		transaction2Request.TransParameters = transaction2FindFirst2Request.GetParameters(m_client.Unicode);
		transaction2Request.TransData = transaction2FindFirst2Request.GetData(m_client.Unicode);
		transaction2Request.TotalDataCount = (ushort)transaction2Request.TransData.Length;
		transaction2Request.TotalParameterCount = (ushort)transaction2Request.TransParameters.Length;
		transaction2Request.MaxParameterCount = 10;
		transaction2Request.MaxDataCount = (ushort)num;
		TrySendMessage(transaction2Request);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION2);
		if (sMB1Message != null)
		{
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is Transaction2Response)
			{
				result = new List<FindInformation>();
				Transaction2Response transaction2Response = (Transaction2Response)sMB1Message.Commands[0];
				Transaction2FindFirst2Response transaction2FindFirst2Response = new Transaction2FindFirst2Response(transaction2Response.TransParameters, transaction2Response.TransData, sMB1Message.Header.UnicodeFlag);
				FindInformationList findInformationList = transaction2FindFirst2Response.GetFindInformationList(transaction2FindFirst2Request.InformationLevel, sMB1Message.Header.UnicodeFlag);
				result.AddRange(findInformationList);
				bool flag = transaction2FindFirst2Response.EndOfSearch;
				while (!flag)
				{
					Transaction2FindNext2Request transaction2FindNext2Request = new Transaction2FindNext2Request();
					transaction2FindNext2Request.SID = transaction2FindFirst2Response.SID;
					transaction2FindNext2Request.SearchCount = ushort.MaxValue;
					transaction2FindNext2Request.Flags = FindFlags.SMB_FIND_CLOSE_AT_EOS | FindFlags.SMB_FIND_CONTINUE_FROM_LAST;
					transaction2FindNext2Request.InformationLevel = informationLevel;
					transaction2FindNext2Request.FileName = fileName;
					transaction2Request = new Transaction2Request();
					transaction2Request.Setup = transaction2FindNext2Request.GetSetup();
					transaction2Request.TransParameters = transaction2FindNext2Request.GetParameters(m_client.Unicode);
					transaction2Request.TransData = transaction2FindNext2Request.GetData(m_client.Unicode);
					transaction2Request.TotalDataCount = (ushort)transaction2Request.TransData.Length;
					transaction2Request.TotalParameterCount = (ushort)transaction2Request.TransParameters.Length;
					transaction2Request.MaxParameterCount = 8;
					transaction2Request.MaxDataCount = (ushort)num;
					TrySendMessage(transaction2Request);
					sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION2);
					if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is Transaction2Response)
					{
						transaction2Response = (Transaction2Response)sMB1Message.Commands[0];
						Transaction2FindNext2Response transaction2FindNext2Response = new Transaction2FindNext2Response(transaction2Response.TransParameters, transaction2Response.TransData, sMB1Message.Header.UnicodeFlag);
						findInformationList = transaction2FindNext2Response.GetFindInformationList(transaction2FindFirst2Request.InformationLevel, sMB1Message.Header.UnicodeFlag);
						result.AddRange(findInformationList);
						flag = transaction2FindNext2Response.EndOfSearch;
					}
					else
					{
						flag = true;
					}
				}
			}
			return sMB1Message.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus GetFileInformation(out FileInformation result, object handle, FileInformationClass informationClass)
	{
		result = null;
		if (m_client.InfoLevelPassthrough)
		{
			int num = 4096;
			Transaction2QueryFileInformationRequest transaction2QueryFileInformationRequest = new Transaction2QueryFileInformationRequest();
			transaction2QueryFileInformationRequest.FID = (ushort)handle;
			transaction2QueryFileInformationRequest.FileInformationClass = informationClass;
			Transaction2Request transaction2Request = new Transaction2Request();
			transaction2Request.Setup = transaction2QueryFileInformationRequest.GetSetup();
			transaction2Request.TransParameters = transaction2QueryFileInformationRequest.GetParameters(m_client.Unicode);
			transaction2Request.TransData = transaction2QueryFileInformationRequest.GetData(m_client.Unicode);
			transaction2Request.TotalDataCount = (ushort)transaction2Request.TransData.Length;
			transaction2Request.TotalParameterCount = (ushort)transaction2Request.TransParameters.Length;
			transaction2Request.MaxParameterCount = 2;
			transaction2Request.MaxDataCount = (ushort)num;
			TrySendMessage(transaction2Request);
			SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION2);
			if (sMB1Message != null)
			{
				if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is Transaction2Response)
				{
					Transaction2Response transaction2Response = (Transaction2Response)sMB1Message.Commands[0];
					Transaction2QueryFileInformationResponse transaction2QueryFileInformationResponse = new Transaction2QueryFileInformationResponse(transaction2Response.TransParameters, transaction2Response.TransData, sMB1Message.Header.UnicodeFlag);
					if (informationClass == FileInformationClass.FileAllInformation)
					{
						QueryInformation queryInformation = transaction2QueryFileInformationResponse.GetQueryInformation(QueryInformationLevel.SMB_QUERY_FILE_ALL_INFO);
						result = QueryInformationHelper.ToFileInformation(queryInformation);
					}
					else
					{
						result = transaction2QueryFileInformationResponse.GetFileInformation(informationClass);
					}
				}
				return sMB1Message.Header.Status;
			}
			return NTStatus.STATUS_INVALID_SMB;
		}
		QueryInformationLevel informationLevel = QueryInformationHelper.ToFileInformationLevel(informationClass);
		QueryInformation result2;
		NTStatus fileInformation = GetFileInformation(out result2, handle, informationLevel);
		if (fileInformation == NTStatus.STATUS_SUCCESS)
		{
			result = QueryInformationHelper.ToFileInformation(result2);
		}
		return fileInformation;
	}

	public NTStatus GetFileInformation(out QueryInformation result, object handle, QueryInformationLevel informationLevel)
	{
		result = null;
		int num = 4096;
		Transaction2QueryFileInformationRequest transaction2QueryFileInformationRequest = new Transaction2QueryFileInformationRequest();
		transaction2QueryFileInformationRequest.FID = (ushort)handle;
		transaction2QueryFileInformationRequest.QueryInformationLevel = informationLevel;
		Transaction2Request transaction2Request = new Transaction2Request();
		transaction2Request.Setup = transaction2QueryFileInformationRequest.GetSetup();
		transaction2Request.TransParameters = transaction2QueryFileInformationRequest.GetParameters(m_client.Unicode);
		transaction2Request.TransData = transaction2QueryFileInformationRequest.GetData(m_client.Unicode);
		transaction2Request.TotalDataCount = (ushort)transaction2Request.TransData.Length;
		transaction2Request.TotalParameterCount = (ushort)transaction2Request.TransParameters.Length;
		transaction2Request.MaxParameterCount = 2;
		transaction2Request.MaxDataCount = (ushort)num;
		TrySendMessage(transaction2Request);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION2);
		if (sMB1Message != null)
		{
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is Transaction2Response)
			{
				Transaction2Response transaction2Response = (Transaction2Response)sMB1Message.Commands[0];
				Transaction2QueryFileInformationResponse transaction2QueryFileInformationResponse = new Transaction2QueryFileInformationResponse(transaction2Response.TransParameters, transaction2Response.TransData, sMB1Message.Header.UnicodeFlag);
				result = transaction2QueryFileInformationResponse.GetQueryInformation(informationLevel);
			}
			return sMB1Message.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus SetFileInformation(object handle, FileInformation information)
	{
		if (m_client.InfoLevelPassthrough)
		{
			if (information is FileRenameInformationType2)
			{
				information = new FileRenameInformationType1
				{
					FileName = ((FileRenameInformationType2)information).FileName,
					ReplaceIfExists = ((FileRenameInformationType2)information).ReplaceIfExists,
					RootDirectory = (uint)((FileRenameInformationType2)information).RootDirectory
				};
			}
			int num = 4096;
			Transaction2SetFileInformationRequest transaction2SetFileInformationRequest = new Transaction2SetFileInformationRequest();
			transaction2SetFileInformationRequest.FID = (ushort)handle;
			transaction2SetFileInformationRequest.SetInformation(information);
			Transaction2Request transaction2Request = new Transaction2Request();
			transaction2Request.Setup = transaction2SetFileInformationRequest.GetSetup();
			transaction2Request.TransParameters = transaction2SetFileInformationRequest.GetParameters(m_client.Unicode);
			transaction2Request.TransData = transaction2SetFileInformationRequest.GetData(m_client.Unicode);
			transaction2Request.TotalDataCount = (ushort)transaction2Request.TransData.Length;
			transaction2Request.TotalParameterCount = (ushort)transaction2Request.TransParameters.Length;
			transaction2Request.MaxParameterCount = 2;
			transaction2Request.MaxDataCount = (ushort)num;
			TrySendMessage(transaction2Request);
			return m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION2)?.Header.Status ?? NTStatus.STATUS_INVALID_SMB;
		}
		throw new NotSupportedException("Server does not support InfoLevelPassthrough");
	}

	public NTStatus SetFileInformation(object handle, SetInformation information)
	{
		int num = 4096;
		Transaction2SetFileInformationRequest transaction2SetFileInformationRequest = new Transaction2SetFileInformationRequest();
		transaction2SetFileInformationRequest.FID = (ushort)handle;
		transaction2SetFileInformationRequest.SetInformation(information);
		Transaction2Request transaction2Request = new Transaction2Request();
		transaction2Request.Setup = transaction2SetFileInformationRequest.GetSetup();
		transaction2Request.TransParameters = transaction2SetFileInformationRequest.GetParameters(m_client.Unicode);
		transaction2Request.TransData = transaction2SetFileInformationRequest.GetData(m_client.Unicode);
		transaction2Request.TotalDataCount = (ushort)transaction2Request.TransData.Length;
		transaction2Request.TotalParameterCount = (ushort)transaction2Request.TransParameters.Length;
		transaction2Request.MaxParameterCount = 2;
		transaction2Request.MaxDataCount = (ushort)num;
		TrySendMessage(transaction2Request);
		return m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION2)?.Header.Status ?? NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus GetFileSystemInformation(out FileSystemInformation result, FileSystemInformationClass informationClass)
	{
		if (m_client.InfoLevelPassthrough)
		{
			result = null;
			int num = 4096;
			Transaction2QueryFSInformationRequest transaction2QueryFSInformationRequest = new Transaction2QueryFSInformationRequest();
			transaction2QueryFSInformationRequest.FileSystemInformationClass = informationClass;
			Transaction2Request transaction2Request = new Transaction2Request();
			transaction2Request.Setup = transaction2QueryFSInformationRequest.GetSetup();
			transaction2Request.TransParameters = transaction2QueryFSInformationRequest.GetParameters(m_client.Unicode);
			transaction2Request.TransData = transaction2QueryFSInformationRequest.GetData(m_client.Unicode);
			transaction2Request.TotalDataCount = (ushort)transaction2Request.TransData.Length;
			transaction2Request.TotalParameterCount = (ushort)transaction2Request.TransParameters.Length;
			transaction2Request.MaxParameterCount = 0;
			transaction2Request.MaxDataCount = (ushort)num;
			TrySendMessage(transaction2Request);
			SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION2);
			if (sMB1Message != null)
			{
				if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is Transaction2Response)
				{
					Transaction2Response transaction2Response = (Transaction2Response)sMB1Message.Commands[0];
					Transaction2QueryFSInformationResponse transaction2QueryFSInformationResponse = new Transaction2QueryFSInformationResponse(transaction2Response.TransParameters, transaction2Response.TransData, sMB1Message.Header.UnicodeFlag);
					result = transaction2QueryFSInformationResponse.GetFileSystemInformation(informationClass);
				}
				return sMB1Message.Header.Status;
			}
			return NTStatus.STATUS_INVALID_SMB;
		}
		throw new NotSupportedException("Server does not support InfoLevelPassthrough");
	}

	public NTStatus GetFileSystemInformation(out QueryFSInformation result, QueryFSInformationLevel informationLevel)
	{
		result = null;
		int num = 4096;
		Transaction2QueryFSInformationRequest transaction2QueryFSInformationRequest = new Transaction2QueryFSInformationRequest();
		transaction2QueryFSInformationRequest.QueryFSInformationLevel = informationLevel;
		Transaction2Request transaction2Request = new Transaction2Request();
		transaction2Request.Setup = transaction2QueryFSInformationRequest.GetSetup();
		transaction2Request.TransParameters = transaction2QueryFSInformationRequest.GetParameters(m_client.Unicode);
		transaction2Request.TransData = transaction2QueryFSInformationRequest.GetData(m_client.Unicode);
		transaction2Request.TotalDataCount = (ushort)transaction2Request.TransData.Length;
		transaction2Request.TotalParameterCount = (ushort)transaction2Request.TransParameters.Length;
		transaction2Request.MaxParameterCount = 0;
		transaction2Request.MaxDataCount = (ushort)num;
		TrySendMessage(transaction2Request);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION2);
		if (sMB1Message != null)
		{
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is Transaction2Response)
			{
				Transaction2Response transaction2Response = (Transaction2Response)sMB1Message.Commands[0];
				Transaction2QueryFSInformationResponse transaction2QueryFSInformationResponse = new Transaction2QueryFSInformationResponse(transaction2Response.TransParameters, transaction2Response.TransData, sMB1Message.Header.UnicodeFlag);
				result = transaction2QueryFSInformationResponse.GetQueryFSInformation(informationLevel, sMB1Message.Header.UnicodeFlag);
			}
			return sMB1Message.Header.Status;
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
		int maxDataCount = 4096;
		NTTransactQuerySecurityDescriptorRequest nTTransactQuerySecurityDescriptorRequest = new NTTransactQuerySecurityDescriptorRequest();
		nTTransactQuerySecurityDescriptorRequest.FID = (ushort)handle;
		nTTransactQuerySecurityDescriptorRequest.SecurityInfoFields = securityInformation;
		NTTransactRequest nTTransactRequest = new NTTransactRequest();
		nTTransactRequest.Function = nTTransactQuerySecurityDescriptorRequest.SubcommandName;
		nTTransactRequest.Setup = nTTransactQuerySecurityDescriptorRequest.GetSetup();
		nTTransactRequest.TransParameters = nTTransactQuerySecurityDescriptorRequest.GetParameters(m_client.Unicode);
		nTTransactRequest.TransData = nTTransactQuerySecurityDescriptorRequest.GetData();
		nTTransactRequest.TotalDataCount = (uint)nTTransactRequest.TransData.Length;
		nTTransactRequest.TotalParameterCount = (uint)nTTransactRequest.TransParameters.Length;
		nTTransactRequest.MaxParameterCount = 4u;
		nTTransactRequest.MaxDataCount = (uint)maxDataCount;
		TrySendMessage(nTTransactRequest);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_NT_TRANSACT);
		if (sMB1Message != null)
		{
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is NTTransactResponse)
			{
				NTTransactResponse nTTransactResponse = (NTTransactResponse)sMB1Message.Commands[0];
				NTTransactQuerySecurityDescriptorResponse nTTransactQuerySecurityDescriptorResponse = new NTTransactQuerySecurityDescriptorResponse(nTTransactResponse.TransParameters, nTTransactResponse.TransData);
				result = nTTransactQuerySecurityDescriptorResponse.SecurityDescriptor;
			}
			return sMB1Message.Header.Status;
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
		if (ctlCode == 1163287)
		{
			return FsCtlPipeTranscieve(handle, input, out output, maxOutputLength);
		}
		output = null;
		NTTransactIOCTLRequest nTTransactIOCTLRequest = new NTTransactIOCTLRequest();
		nTTransactIOCTLRequest.FID = (ushort)handle;
		nTTransactIOCTLRequest.FunctionCode = ctlCode;
		nTTransactIOCTLRequest.IsFsctl = true;
		nTTransactIOCTLRequest.Data = input;
		NTTransactRequest nTTransactRequest = new NTTransactRequest();
		nTTransactRequest.Function = nTTransactIOCTLRequest.SubcommandName;
		nTTransactRequest.Setup = nTTransactIOCTLRequest.GetSetup();
		nTTransactRequest.TransParameters = nTTransactIOCTLRequest.GetParameters(m_client.Unicode);
		nTTransactRequest.TransData = nTTransactIOCTLRequest.GetData();
		nTTransactRequest.TotalDataCount = (uint)nTTransactRequest.TransData.Length;
		nTTransactRequest.TotalParameterCount = (uint)nTTransactRequest.TransParameters.Length;
		nTTransactRequest.MaxParameterCount = 0u;
		nTTransactRequest.MaxDataCount = (uint)maxOutputLength;
		TrySendMessage(nTTransactRequest);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_NT_TRANSACT);
		if (sMB1Message != null)
		{
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is NTTransactResponse)
			{
				NTTransactResponse nTTransactResponse = (NTTransactResponse)sMB1Message.Commands[0];
				NTTransactIOCTLResponse nTTransactIOCTLResponse = new NTTransactIOCTLResponse(nTTransactResponse.Setup, nTTransactResponse.TransData);
				output = nTTransactIOCTLResponse.Data;
			}
			return sMB1Message.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus FsCtlPipeTranscieve(object handle, byte[] input, out byte[] output, int maxOutputLength)
	{
		output = null;
		TransactionTransactNamedPipeRequest transactionTransactNamedPipeRequest = new TransactionTransactNamedPipeRequest();
		transactionTransactNamedPipeRequest.FID = (ushort)handle;
		transactionTransactNamedPipeRequest.WriteData = input;
		TransactionRequest transactionRequest = new TransactionRequest();
		transactionRequest.Setup = transactionTransactNamedPipeRequest.GetSetup();
		transactionRequest.TransParameters = transactionTransactNamedPipeRequest.GetParameters();
		transactionRequest.TransData = transactionTransactNamedPipeRequest.GetData(m_client.Unicode);
		transactionRequest.TotalDataCount = (ushort)transactionRequest.TransData.Length;
		transactionRequest.TotalParameterCount = (ushort)transactionRequest.TransParameters.Length;
		transactionRequest.MaxParameterCount = 0;
		transactionRequest.MaxDataCount = (ushort)maxOutputLength;
		transactionRequest.Name = "\\PIPE\\";
		TrySendMessage(transactionRequest);
		SMB1Message sMB1Message = m_client.WaitForMessage(CommandName.SMB_COM_TRANSACTION);
		if (sMB1Message != null)
		{
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is TransactionResponse)
			{
				TransactionTransactNamedPipeResponse transactionTransactNamedPipeResponse = new TransactionTransactNamedPipeResponse(((TransactionResponse)sMB1Message.Commands[0]).TransData);
				output = transactionTransactNamedPipeResponse.ReadData;
			}
			return sMB1Message.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus Disconnect()
	{
		TreeDisconnectRequest request = new TreeDisconnectRequest();
		TrySendMessage(request);
		return m_client.WaitForMessage(CommandName.SMB_COM_TREE_DISCONNECT)?.Header.Status ?? NTStatus.STATUS_INVALID_SMB;
	}

	private void TrySendMessage(SMB1Command request)
	{
		if (!m_client.IsConnected)
		{
			throw new InvalidOperationException("The client is no longer connected");
		}
		m_client.TrySendMessage(request, m_treeID);
	}

	private static ExtendedFileAttributes ToExtendedFileAttributes(FileAttributes fileAttributes)
	{
		return (ExtendedFileAttributes)((FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive | FileAttributes.Normal | FileAttributes.Temporary | FileAttributes.Offline | FileAttributes.Encrypted) & fileAttributes);
	}

	private static FileStatus ToFileStatus(CreateDisposition createDisposition)
	{
		return createDisposition switch
		{
			CreateDisposition.FILE_SUPERSEDE => FileStatus.FILE_SUPERSEDED, 
			CreateDisposition.FILE_OPEN => FileStatus.FILE_OPENED, 
			CreateDisposition.FILE_CREATE => FileStatus.FILE_CREATED, 
			CreateDisposition.FILE_OPEN_IF => FileStatus.FILE_OVERWRITTEN, 
			CreateDisposition.FILE_OVERWRITE => FileStatus.FILE_EXISTS, 
			CreateDisposition.FILE_OVERWRITE_IF => FileStatus.FILE_DOES_NOT_EXIST, 
			_ => FileStatus.FILE_OPENED, 
		};
	}
}
