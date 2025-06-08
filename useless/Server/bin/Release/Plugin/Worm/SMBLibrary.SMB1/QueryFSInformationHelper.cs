using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFSInformationHelper
{
	public static FileSystemInformationClass ToFileSystemInformationClass(QueryFSInformationLevel informationLevel)
	{
		return informationLevel switch
		{
			QueryFSInformationLevel.SMB_QUERY_FS_VOLUME_INFO => FileSystemInformationClass.FileFsVolumeInformation, 
			QueryFSInformationLevel.SMB_QUERY_FS_SIZE_INFO => FileSystemInformationClass.FileFsSizeInformation, 
			QueryFSInformationLevel.SMB_QUERY_FS_DEVICE_INFO => FileSystemInformationClass.FileFsDeviceInformation, 
			QueryFSInformationLevel.SMB_QUERY_FS_ATTRIBUTE_INFO => FileSystemInformationClass.FileFsAttributeInformation, 
			_ => throw new UnsupportedInformationLevelException(), 
		};
	}

	public static QueryFSInformation FromFileSystemInformation(FileSystemInformation fsInfo)
	{
		if (fsInfo is FileFsVolumeInformation)
		{
			FileFsVolumeInformation fileFsVolumeInformation = (FileFsVolumeInformation)fsInfo;
			return new QueryFSVolumeInfo
			{
				VolumeCreationTime = fileFsVolumeInformation.VolumeCreationTime,
				SerialNumber = fileFsVolumeInformation.VolumeSerialNumber,
				VolumeLabel = fileFsVolumeInformation.VolumeLabel
			};
		}
		if (fsInfo is FileFsSizeInformation)
		{
			FileFsSizeInformation fileFsSizeInformation = (FileFsSizeInformation)fsInfo;
			return new QueryFSSizeInfo
			{
				TotalAllocationUnits = fileFsSizeInformation.TotalAllocationUnits,
				TotalFreeAllocationUnits = fileFsSizeInformation.AvailableAllocationUnits,
				BytesPerSector = fileFsSizeInformation.BytesPerSector,
				SectorsPerAllocationUnit = fileFsSizeInformation.SectorsPerAllocationUnit
			};
		}
		if (fsInfo is FileFsDeviceInformation)
		{
			FileFsDeviceInformation fileFsDeviceInformation = (FileFsDeviceInformation)fsInfo;
			return new QueryFSDeviceInfo
			{
				DeviceType = fileFsDeviceInformation.DeviceType,
				DeviceCharacteristics = fileFsDeviceInformation.Characteristics
			};
		}
		if (fsInfo is FileFsAttributeInformation)
		{
			FileFsAttributeInformation fileFsAttributeInformation = (FileFsAttributeInformation)fsInfo;
			return new QueryFSAttibuteInfo
			{
				FileSystemAttributes = fileFsAttributeInformation.FileSystemAttributes,
				MaxFileNameLengthInBytes = fileFsAttributeInformation.MaximumComponentNameLength,
				FileSystemName = fileFsAttributeInformation.FileSystemName
			};
		}
		throw new NotImplementedException();
	}
}
