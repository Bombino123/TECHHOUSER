using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileValidDataLengthInformation : FileInformation
{
	public const int FixedLength = 8;

	public long ValidDataLength;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileValidDataLengthInformation;

	public override int Length => 8;

	public FileValidDataLengthInformation()
	{
	}

	public FileValidDataLengthInformation(byte[] buffer, int offset)
	{
		ValidDataLength = LittleEndianConverter.ToInt64(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, ValidDataLength);
	}
}
