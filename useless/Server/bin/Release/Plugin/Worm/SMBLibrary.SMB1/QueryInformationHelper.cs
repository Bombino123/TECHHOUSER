using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryInformationHelper
{
	public static FileInformationClass ToFileInformationClass(QueryInformationLevel informationLevel)
	{
		return informationLevel switch
		{
			QueryInformationLevel.SMB_QUERY_FILE_BASIC_INFO => FileInformationClass.FileBasicInformation, 
			QueryInformationLevel.SMB_QUERY_FILE_STANDARD_INFO => FileInformationClass.FileStandardInformation, 
			QueryInformationLevel.SMB_QUERY_FILE_EA_INFO => FileInformationClass.FileEaInformation, 
			QueryInformationLevel.SMB_QUERY_FILE_NAME_INFO => FileInformationClass.FileNameInformation, 
			QueryInformationLevel.SMB_QUERY_FILE_ALL_INFO => FileInformationClass.FileAllInformation, 
			QueryInformationLevel.SMB_QUERY_FILE_ALT_NAME_INFO => FileInformationClass.FileAlternateNameInformation, 
			QueryInformationLevel.SMB_QUERY_FILE_STREAM_INFO => FileInformationClass.FileStreamInformation, 
			QueryInformationLevel.SMB_QUERY_FILE_COMPRESSION_INFO => FileInformationClass.FileCompressionInformation, 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}

	public static QueryInformation FromFileInformation(FileInformation fileInformation)
	{
		if (fileInformation is FileBasicInformation)
		{
			FileBasicInformation fileBasicInformation = (FileBasicInformation)fileInformation;
			return new QueryFileBasicInfo
			{
				CreationTime = fileBasicInformation.CreationTime,
				LastAccessTime = fileBasicInformation.LastAccessTime,
				LastWriteTime = fileBasicInformation.LastWriteTime,
				LastChangeTime = fileBasicInformation.ChangeTime,
				ExtFileAttributes = (ExtendedFileAttributes)fileBasicInformation.FileAttributes
			};
		}
		if (fileInformation is FileStandardInformation)
		{
			FileStandardInformation fileStandardInformation = (FileStandardInformation)fileInformation;
			return new QueryFileStandardInfo
			{
				AllocationSize = fileStandardInformation.AllocationSize,
				EndOfFile = fileStandardInformation.EndOfFile,
				DeletePending = fileStandardInformation.DeletePending,
				Directory = fileStandardInformation.Directory
			};
		}
		if (fileInformation is FileEaInformation)
		{
			FileEaInformation fileEaInformation = (FileEaInformation)fileInformation;
			return new QueryFileEaInfo
			{
				EaSize = fileEaInformation.EaSize
			};
		}
		if (fileInformation is FileNameInformation)
		{
			FileNameInformation fileNameInformation = (FileNameInformation)fileInformation;
			return new QueryFileNameInfo
			{
				FileName = fileNameInformation.FileName
			};
		}
		if (fileInformation is FileAllInformation)
		{
			FileAllInformation fileAllInformation = (FileAllInformation)fileInformation;
			return new QueryFileAllInfo
			{
				CreationTime = fileAllInformation.BasicInformation.CreationTime,
				LastAccessTime = fileAllInformation.BasicInformation.LastAccessTime,
				LastWriteTime = fileAllInformation.BasicInformation.LastWriteTime,
				LastChangeTime = fileAllInformation.BasicInformation.ChangeTime,
				ExtFileAttributes = (ExtendedFileAttributes)fileAllInformation.BasicInformation.FileAttributes,
				AllocationSize = fileAllInformation.StandardInformation.AllocationSize,
				EndOfFile = fileAllInformation.StandardInformation.EndOfFile,
				DeletePending = fileAllInformation.StandardInformation.DeletePending,
				Directory = fileAllInformation.StandardInformation.Directory,
				EaSize = fileAllInformation.EaInformation.EaSize,
				FileName = fileAllInformation.NameInformation.FileName
			};
		}
		if (fileInformation is FileAlternateNameInformation)
		{
			FileAlternateNameInformation fileAlternateNameInformation = (FileAlternateNameInformation)fileInformation;
			return new QueryFileAltNameInfo
			{
				FileName = fileAlternateNameInformation.FileName
			};
		}
		if (fileInformation is FileStreamInformation)
		{
			FileStreamInformation fileStreamInformation = (FileStreamInformation)fileInformation;
			QueryFileStreamInfo queryFileStreamInfo = new QueryFileStreamInfo();
			queryFileStreamInfo.Entries.AddRange(fileStreamInformation.Entries);
			return queryFileStreamInfo;
		}
		if (fileInformation is FileCompressionInformation)
		{
			FileCompressionInformation fileCompressionInformation = (FileCompressionInformation)fileInformation;
			return new QueryFileCompressionInfo
			{
				CompressedFileSize = fileCompressionInformation.CompressedFileSize,
				CompressionFormat = fileCompressionInformation.CompressionFormat,
				CompressionUnitShift = fileCompressionInformation.CompressionUnitShift,
				ChunkShift = fileCompressionInformation.ChunkShift,
				ClusterShift = fileCompressionInformation.ClusterShift,
				Reserved = fileCompressionInformation.Reserved
			};
		}
		throw new NotImplementedException();
	}

	public static QueryInformationLevel ToFileInformationLevel(FileInformationClass informationClass)
	{
		return informationClass switch
		{
			FileInformationClass.FileBasicInformation => QueryInformationLevel.SMB_QUERY_FILE_BASIC_INFO, 
			FileInformationClass.FileStandardInformation => QueryInformationLevel.SMB_QUERY_FILE_STANDARD_INFO, 
			FileInformationClass.FileEaInformation => QueryInformationLevel.SMB_QUERY_FILE_EA_INFO, 
			FileInformationClass.FileNameInformation => QueryInformationLevel.SMB_QUERY_FILE_NAME_INFO, 
			FileInformationClass.FileAllInformation => QueryInformationLevel.SMB_QUERY_FILE_ALL_INFO, 
			FileInformationClass.FileAlternateNameInformation => QueryInformationLevel.SMB_QUERY_FILE_ALT_NAME_INFO, 
			FileInformationClass.FileStreamInformation => QueryInformationLevel.SMB_QUERY_FILE_STREAM_INFO, 
			FileInformationClass.FileCompressionInformation => QueryInformationLevel.SMB_QUERY_FILE_COMPRESSION_INFO, 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}

	public static FileInformation ToFileInformation(QueryInformation queryInformation)
	{
		if (queryInformation is QueryFileBasicInfo)
		{
			QueryFileBasicInfo queryFileBasicInfo = (QueryFileBasicInfo)queryInformation;
			return new FileBasicInformation
			{
				CreationTime = queryFileBasicInfo.CreationTime,
				LastAccessTime = queryFileBasicInfo.LastAccessTime,
				LastWriteTime = queryFileBasicInfo.LastWriteTime,
				ChangeTime = queryFileBasicInfo.LastChangeTime,
				FileAttributes = (FileAttributes)queryFileBasicInfo.ExtFileAttributes
			};
		}
		if (queryInformation is QueryFileStandardInfo)
		{
			QueryFileStandardInfo queryFileStandardInfo = (QueryFileStandardInfo)queryInformation;
			return new FileStandardInformation
			{
				AllocationSize = queryFileStandardInfo.AllocationSize,
				EndOfFile = queryFileStandardInfo.EndOfFile,
				DeletePending = queryFileStandardInfo.DeletePending,
				Directory = queryFileStandardInfo.Directory
			};
		}
		if (queryInformation is QueryFileEaInfo)
		{
			QueryFileEaInfo queryFileEaInfo = (QueryFileEaInfo)queryInformation;
			return new FileEaInformation
			{
				EaSize = queryFileEaInfo.EaSize
			};
		}
		if (queryInformation is QueryFileNameInfo)
		{
			QueryFileNameInfo queryFileNameInfo = (QueryFileNameInfo)queryInformation;
			return new FileNameInformation
			{
				FileName = queryFileNameInfo.FileName
			};
		}
		if (queryInformation is QueryFileAllInfo)
		{
			QueryFileAllInfo queryFileAllInfo = (QueryFileAllInfo)queryInformation;
			return new FileAllInformation
			{
				BasicInformation = 
				{
					CreationTime = queryFileAllInfo.CreationTime,
					LastAccessTime = queryFileAllInfo.LastAccessTime,
					LastWriteTime = queryFileAllInfo.LastWriteTime,
					ChangeTime = queryFileAllInfo.LastChangeTime,
					FileAttributes = (FileAttributes)queryFileAllInfo.ExtFileAttributes
				},
				StandardInformation = 
				{
					AllocationSize = queryFileAllInfo.AllocationSize,
					EndOfFile = queryFileAllInfo.EndOfFile,
					DeletePending = queryFileAllInfo.DeletePending,
					Directory = queryFileAllInfo.Directory
				},
				EaInformation = 
				{
					EaSize = queryFileAllInfo.EaSize
				},
				NameInformation = 
				{
					FileName = queryFileAllInfo.FileName
				}
			};
		}
		if (queryInformation is QueryFileAltNameInfo)
		{
			QueryFileAltNameInfo queryFileAltNameInfo = (QueryFileAltNameInfo)queryInformation;
			return new FileAlternateNameInformation
			{
				FileName = queryFileAltNameInfo.FileName
			};
		}
		if (queryInformation is QueryFileStreamInfo)
		{
			QueryFileStreamInfo queryFileStreamInfo = (QueryFileStreamInfo)queryInformation;
			FileStreamInformation fileStreamInformation = new FileStreamInformation();
			fileStreamInformation.Entries.AddRange(queryFileStreamInfo.Entries);
			return fileStreamInformation;
		}
		if (queryInformation is QueryFileCompressionInfo)
		{
			QueryFileCompressionInfo queryFileCompressionInfo = (QueryFileCompressionInfo)queryInformation;
			return new FileCompressionInformation
			{
				CompressedFileSize = queryFileCompressionInfo.CompressedFileSize,
				CompressionFormat = queryFileCompressionInfo.CompressionFormat,
				CompressionUnitShift = queryFileCompressionInfo.CompressionUnitShift,
				ChunkShift = queryFileCompressionInfo.ChunkShift,
				ClusterShift = queryFileCompressionInfo.ClusterShift,
				Reserved = queryFileCompressionInfo.Reserved
			};
		}
		throw new NotImplementedException();
	}
}
