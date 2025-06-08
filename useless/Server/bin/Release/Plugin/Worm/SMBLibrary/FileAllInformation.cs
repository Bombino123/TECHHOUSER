using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public class FileAllInformation : FileInformation
{
	public FileBasicInformation BasicInformation;

	public FileStandardInformation StandardInformation;

	public FileInternalInformation InternalInformation;

	public FileEaInformation EaInformation;

	public FileAccessInformation AccessInformation;

	public FilePositionInformation PositionInformation;

	public FileModeInformation ModeInformation;

	public FileAlignmentInformation AlignmentInformation;

	public FileNameInformation NameInformation;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileAllInformation;

	public override int Length => 96 + NameInformation.Length;

	public FileAllInformation()
	{
		BasicInformation = new FileBasicInformation();
		StandardInformation = new FileStandardInformation();
		InternalInformation = new FileInternalInformation();
		EaInformation = new FileEaInformation();
		AccessInformation = new FileAccessInformation();
		PositionInformation = new FilePositionInformation();
		ModeInformation = new FileModeInformation();
		AlignmentInformation = new FileAlignmentInformation();
		NameInformation = new FileNameInformation();
	}

	public FileAllInformation(byte[] buffer, int offset)
	{
		BasicInformation = new FileBasicInformation(buffer, offset);
		StandardInformation = new FileStandardInformation(buffer, offset + 40);
		InternalInformation = new FileInternalInformation(buffer, offset + 64);
		EaInformation = new FileEaInformation(buffer, offset + 72);
		AccessInformation = new FileAccessInformation(buffer, offset + 76);
		PositionInformation = new FilePositionInformation(buffer, offset + 80);
		ModeInformation = new FileModeInformation(buffer, offset + 88);
		AlignmentInformation = new FileAlignmentInformation(buffer, offset + 92);
		NameInformation = new FileNameInformation(buffer, offset + 96);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		BasicInformation.WriteBytes(buffer, offset);
		StandardInformation.WriteBytes(buffer, offset + 40);
		InternalInformation.WriteBytes(buffer, offset + 64);
		EaInformation.WriteBytes(buffer, offset + 72);
		AccessInformation.WriteBytes(buffer, offset + 76);
		PositionInformation.WriteBytes(buffer, offset + 80);
		ModeInformation.WriteBytes(buffer, offset + 88);
		AlignmentInformation.WriteBytes(buffer, offset + 92);
		NameInformation.WriteBytes(buffer, offset + 96);
	}
}
