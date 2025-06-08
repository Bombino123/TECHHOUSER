using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileEndOfFileInformation : FileInformation
{
	public const int FixedLength = 8;

	public long EndOfFile;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileEndOfFileInformation;

	public override int Length => 8;

	public FileEndOfFileInformation()
	{
	}

	public FileEndOfFileInformation(byte[] buffer, int offset)
	{
		EndOfFile = LittleEndianConverter.ToInt64(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, EndOfFile);
	}
}
