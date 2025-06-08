using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FindInformationHelper
{
	public static FileInformationClass ToFileInformationClass(FindInformationLevel informationLevel)
	{
		return informationLevel switch
		{
			FindInformationLevel.SMB_FIND_FILE_DIRECTORY_INFO => FileInformationClass.FileDirectoryInformation, 
			FindInformationLevel.SMB_FIND_FILE_FULL_DIRECTORY_INFO => FileInformationClass.FileFullDirectoryInformation, 
			FindInformationLevel.SMB_FIND_FILE_NAMES_INFO => FileInformationClass.FileNamesInformation, 
			FindInformationLevel.SMB_FIND_FILE_BOTH_DIRECTORY_INFO => FileInformationClass.FileBothDirectoryInformation, 
			FindInformationLevel.SMB_FIND_FILE_ID_FULL_DIRECTORY_INFO => FileInformationClass.FileIdFullDirectoryInformation, 
			FindInformationLevel.SMB_FIND_FILE_ID_BOTH_DIRECTORY_INFO => FileInformationClass.FileIdBothDirectoryInformation, 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}

	public static FindInformationList ToFindInformationList(List<QueryDirectoryFileInformation> entries, bool isUnicode, int maxLength)
	{
		FindInformationList findInformationList = new FindInformationList();
		int num = 0;
		for (int i = 0; i < entries.Count; i++)
		{
			FindInformation findInformation = ToFindInformation(entries[i]);
			int length = findInformation.GetLength(isUnicode);
			if (num + length > maxLength)
			{
				break;
			}
			findInformationList.Add(findInformation);
			num += length;
		}
		return findInformationList;
	}

	public static FindInformation ToFindInformation(QueryDirectoryFileInformation fileInformation)
	{
		if (fileInformation is FileDirectoryInformation)
		{
			FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)fileInformation;
			return new FindFileDirectoryInfo
			{
				FileIndex = fileDirectoryInformation.FileIndex,
				CreationTime = fileDirectoryInformation.CreationTime,
				LastAccessTime = fileDirectoryInformation.LastAccessTime,
				LastWriteTime = fileDirectoryInformation.LastWriteTime,
				LastAttrChangeTime = fileDirectoryInformation.LastWriteTime,
				EndOfFile = fileDirectoryInformation.EndOfFile,
				AllocationSize = fileDirectoryInformation.AllocationSize,
				ExtFileAttributes = (ExtendedFileAttributes)fileDirectoryInformation.FileAttributes,
				FileName = fileDirectoryInformation.FileName
			};
		}
		if (fileInformation is FileFullDirectoryInformation)
		{
			FileFullDirectoryInformation fileFullDirectoryInformation = (FileFullDirectoryInformation)fileInformation;
			return new FindFileFullDirectoryInfo
			{
				FileIndex = fileFullDirectoryInformation.FileIndex,
				CreationTime = fileFullDirectoryInformation.CreationTime,
				LastAccessTime = fileFullDirectoryInformation.LastAccessTime,
				LastWriteTime = fileFullDirectoryInformation.LastWriteTime,
				LastAttrChangeTime = fileFullDirectoryInformation.LastWriteTime,
				EndOfFile = fileFullDirectoryInformation.EndOfFile,
				AllocationSize = fileFullDirectoryInformation.AllocationSize,
				ExtFileAttributes = (ExtendedFileAttributes)fileFullDirectoryInformation.FileAttributes,
				EASize = fileFullDirectoryInformation.EaSize,
				FileName = fileFullDirectoryInformation.FileName
			};
		}
		if (fileInformation is FileNamesInformation)
		{
			FileNamesInformation fileNamesInformation = (FileNamesInformation)fileInformation;
			return new FindFileNamesInfo
			{
				FileIndex = fileNamesInformation.FileIndex,
				FileName = fileNamesInformation.FileName
			};
		}
		if (fileInformation is FileBothDirectoryInformation)
		{
			FileBothDirectoryInformation fileBothDirectoryInformation = (FileBothDirectoryInformation)fileInformation;
			return new FindFileBothDirectoryInfo
			{
				FileIndex = fileBothDirectoryInformation.FileIndex,
				CreationTime = fileBothDirectoryInformation.CreationTime,
				LastAccessTime = fileBothDirectoryInformation.LastAccessTime,
				LastWriteTime = fileBothDirectoryInformation.LastWriteTime,
				LastChangeTime = fileBothDirectoryInformation.LastWriteTime,
				EndOfFile = fileBothDirectoryInformation.EndOfFile,
				AllocationSize = fileBothDirectoryInformation.AllocationSize,
				ExtFileAttributes = (ExtendedFileAttributes)fileBothDirectoryInformation.FileAttributes,
				EASize = fileBothDirectoryInformation.EaSize,
				Reserved = fileBothDirectoryInformation.Reserved,
				ShortName = fileBothDirectoryInformation.ShortName,
				FileName = fileBothDirectoryInformation.FileName
			};
		}
		if (fileInformation is FileIdFullDirectoryInformation)
		{
			FileIdFullDirectoryInformation fileIdFullDirectoryInformation = (FileIdFullDirectoryInformation)fileInformation;
			return new FindFileIDFullDirectoryInfo
			{
				FileIndex = fileIdFullDirectoryInformation.FileIndex,
				CreationTime = fileIdFullDirectoryInformation.CreationTime,
				LastAccessTime = fileIdFullDirectoryInformation.LastAccessTime,
				LastWriteTime = fileIdFullDirectoryInformation.LastWriteTime,
				LastAttrChangeTime = fileIdFullDirectoryInformation.LastWriteTime,
				EndOfFile = fileIdFullDirectoryInformation.EndOfFile,
				AllocationSize = fileIdFullDirectoryInformation.AllocationSize,
				ExtFileAttributes = (ExtendedFileAttributes)fileIdFullDirectoryInformation.FileAttributes,
				EASize = fileIdFullDirectoryInformation.EaSize,
				Reserved = fileIdFullDirectoryInformation.Reserved,
				FileID = fileIdFullDirectoryInformation.FileId,
				FileName = fileIdFullDirectoryInformation.FileName
			};
		}
		if (fileInformation is FileIdBothDirectoryInformation)
		{
			FileIdBothDirectoryInformation fileIdBothDirectoryInformation = (FileIdBothDirectoryInformation)fileInformation;
			return new FindFileIDBothDirectoryInfo
			{
				FileIndex = fileIdBothDirectoryInformation.FileIndex,
				CreationTime = fileIdBothDirectoryInformation.CreationTime,
				LastAccessTime = fileIdBothDirectoryInformation.LastAccessTime,
				LastWriteTime = fileIdBothDirectoryInformation.LastWriteTime,
				LastChangeTime = fileIdBothDirectoryInformation.LastWriteTime,
				EndOfFile = fileIdBothDirectoryInformation.EndOfFile,
				AllocationSize = fileIdBothDirectoryInformation.AllocationSize,
				ExtFileAttributes = (ExtendedFileAttributes)fileIdBothDirectoryInformation.FileAttributes,
				EASize = fileIdBothDirectoryInformation.EaSize,
				Reserved = fileIdBothDirectoryInformation.Reserved1,
				ShortName = fileIdBothDirectoryInformation.ShortName,
				Reserved2 = fileIdBothDirectoryInformation.Reserved2,
				FileID = fileIdBothDirectoryInformation.FileId,
				FileName = fileIdBothDirectoryInformation.FileName
			};
		}
		throw new NotImplementedException();
	}
}
