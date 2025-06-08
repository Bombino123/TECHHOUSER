using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public abstract class FileInformation
{
	public abstract FileInformationClass FileInformationClass { get; }

	public abstract int Length { get; }

	public abstract void WriteBytes(byte[] buffer, int offset);

	public byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		WriteBytes(array, 0);
		return array;
	}

	public static FileInformation GetFileInformation(byte[] buffer, int offset, FileInformationClass informationClass)
	{
		return informationClass switch
		{
			FileInformationClass.FileBasicInformation => new FileBasicInformation(buffer, offset), 
			FileInformationClass.FileStandardInformation => new FileStandardInformation(buffer, offset), 
			FileInformationClass.FileInternalInformation => new FileInternalInformation(buffer, offset), 
			FileInformationClass.FileEaInformation => new FileEaInformation(buffer, offset), 
			FileInformationClass.FileAccessInformation => new FileAccessInformation(buffer, offset), 
			FileInformationClass.FileRenameInformation => new FileRenameInformationType2(buffer, offset), 
			FileInformationClass.FileLinkInformation => new FileLinkInformationType2(buffer, offset), 
			FileInformationClass.FileNamesInformation => throw new NotImplementedException(), 
			FileInformationClass.FileDispositionInformation => new FileDispositionInformation(buffer, offset), 
			FileInformationClass.FilePositionInformation => new FilePositionInformation(buffer, offset), 
			FileInformationClass.FileFullEaInformation => new FileFullEAInformation(buffer, offset), 
			FileInformationClass.FileModeInformation => new FileModeInformation(buffer, offset), 
			FileInformationClass.FileAlignmentInformation => new FileAlignmentInformation(buffer, offset), 
			FileInformationClass.FileAllInformation => new FileAllInformation(buffer, offset), 
			FileInformationClass.FileAllocationInformation => new FileAllocationInformation(buffer, offset), 
			FileInformationClass.FileEndOfFileInformation => new FileEndOfFileInformation(buffer, offset), 
			FileInformationClass.FileAlternateNameInformation => new FileAlternateNameInformation(buffer, offset), 
			FileInformationClass.FileStreamInformation => new FileStreamInformation(buffer, offset), 
			FileInformationClass.FilePipeInformation => throw new NotImplementedException(), 
			FileInformationClass.FilePipeLocalInformation => throw new NotImplementedException(), 
			FileInformationClass.FilePipeRemoteInformation => throw new NotImplementedException(), 
			FileInformationClass.FileCompressionInformation => new FileCompressionInformation(buffer, offset), 
			FileInformationClass.FileNetworkOpenInformation => new FileNetworkOpenInformation(buffer, offset), 
			FileInformationClass.FileAttributeTagInformation => throw new NotImplementedException(), 
			FileInformationClass.FileValidDataLengthInformation => new FileValidDataLengthInformation(buffer, offset), 
			FileInformationClass.FileShortNameInformation => throw new NotImplementedException(), 
			_ => throw new UnsupportedInformationLevelException($"Unsupported information class: {informationClass}"), 
		};
	}
}
