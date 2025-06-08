using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class FindInformation
{
	public uint NextEntryOffset;

	public abstract FindInformationLevel InformationLevel { get; }

	public FindInformation()
	{
	}

	public abstract void WriteBytes(byte[] buffer, ref int offset, bool isUnicode);

	public abstract int GetLength(bool isUnicode);

	public static FindInformation ReadEntry(byte[] buffer, int offset, FindInformationLevel informationLevel, bool isUnicode)
	{
		return informationLevel switch
		{
			FindInformationLevel.SMB_FIND_FILE_DIRECTORY_INFO => new FindFileDirectoryInfo(buffer, offset, isUnicode), 
			FindInformationLevel.SMB_FIND_FILE_FULL_DIRECTORY_INFO => new FindFileFullDirectoryInfo(buffer, offset, isUnicode), 
			FindInformationLevel.SMB_FIND_FILE_NAMES_INFO => new FindFileNamesInfo(buffer, offset, isUnicode), 
			FindInformationLevel.SMB_FIND_FILE_BOTH_DIRECTORY_INFO => new FindFileBothDirectoryInfo(buffer, offset, isUnicode), 
			FindInformationLevel.SMB_FIND_FILE_ID_FULL_DIRECTORY_INFO => new FindFileIDFullDirectoryInfo(buffer, offset, isUnicode), 
			FindInformationLevel.SMB_FIND_FILE_ID_BOTH_DIRECTORY_INFO => new FindFileIDBothDirectoryInfo(buffer, offset, isUnicode), 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}
}
