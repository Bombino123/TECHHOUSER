using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SetInformationHelper
{
	public static FileInformation ToFileInformation(SetInformation information)
	{
		if (information is SetFileBasicInfo)
		{
			SetFileBasicInfo setFileBasicInfo = (SetFileBasicInfo)information;
			return new FileBasicInformation
			{
				CreationTime = setFileBasicInfo.CreationTime,
				LastAccessTime = setFileBasicInfo.LastAccessTime,
				LastWriteTime = setFileBasicInfo.LastWriteTime,
				ChangeTime = setFileBasicInfo.LastChangeTime,
				FileAttributes = (FileAttributes)setFileBasicInfo.ExtFileAttributes,
				Reserved = setFileBasicInfo.Reserved
			};
		}
		if (information is SetFileDispositionInfo)
		{
			return new FileDispositionInformation
			{
				DeletePending = ((SetFileDispositionInfo)information).DeletePending
			};
		}
		if (information is SetFileAllocationInfo)
		{
			return new FileAllocationInformation
			{
				AllocationSize = ((SetFileAllocationInfo)information).AllocationSize
			};
		}
		if (information is SetFileEndOfFileInfo)
		{
			return new FileEndOfFileInformation
			{
				EndOfFile = ((SetFileEndOfFileInfo)information).EndOfFile
			};
		}
		throw new NotImplementedException();
	}
}
