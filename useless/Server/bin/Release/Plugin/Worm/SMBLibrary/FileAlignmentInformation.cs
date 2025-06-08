using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileAlignmentInformation : FileInformation
{
	public const int FixedLength = 4;

	public uint AlignmentRequirement;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileAlignmentInformation;

	public override int Length => 4;

	public FileAlignmentInformation()
	{
	}

	public FileAlignmentInformation(byte[] buffer, int offset)
	{
		AlignmentRequirement = LittleEndianConverter.ToUInt32(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, offset, AlignmentRequirement);
	}
}
