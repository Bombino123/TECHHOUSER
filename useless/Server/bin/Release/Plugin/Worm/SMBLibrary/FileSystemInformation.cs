using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public abstract class FileSystemInformation
{
	public abstract FileSystemInformationClass FileSystemInformationClass { get; }

	public abstract int Length { get; }

	public abstract void WriteBytes(byte[] buffer, int offset);

	public byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		WriteBytes(array, 0);
		return array;
	}

	public static FileSystemInformation GetFileSystemInformation(byte[] buffer, int offset, FileSystemInformationClass informationClass)
	{
		return informationClass switch
		{
			FileSystemInformationClass.FileFsVolumeInformation => new FileFsVolumeInformation(buffer, offset), 
			FileSystemInformationClass.FileFsSizeInformation => new FileFsSizeInformation(buffer, offset), 
			FileSystemInformationClass.FileFsDeviceInformation => new FileFsDeviceInformation(buffer, offset), 
			FileSystemInformationClass.FileFsAttributeInformation => new FileFsAttributeInformation(buffer, offset), 
			FileSystemInformationClass.FileFsControlInformation => new FileFsControlInformation(buffer, offset), 
			FileSystemInformationClass.FileFsFullSizeInformation => new FileFsFullSizeInformation(buffer, offset), 
			FileSystemInformationClass.FileFsObjectIdInformation => new FileFsObjectIdInformation(buffer, offset), 
			FileSystemInformationClass.FileFsSectorSizeInformation => new FileFsSectorSizeInformation(buffer, offset), 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}
}
