using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class SetInformation
{
	public abstract SetInformationLevel InformationLevel { get; }

	public abstract byte[] GetBytes();

	public static SetInformation GetSetInformation(byte[] buffer, SetInformationLevel informationLevel)
	{
		return informationLevel switch
		{
			SetInformationLevel.SMB_SET_FILE_BASIC_INFO => new SetFileBasicInfo(buffer), 
			SetInformationLevel.SMB_SET_FILE_DISPOSITION_INFO => new SetFileDispositionInfo(buffer), 
			SetInformationLevel.SMB_SET_FILE_ALLOCATION_INFO => new SetFileAllocationInfo(buffer), 
			SetInformationLevel.SMB_SET_FILE_END_OF_FILE_INFO => new SetFileEndOfFileInfo(buffer), 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}
}
