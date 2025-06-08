using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FilePositionInformation : FileInformation
{
	public const int FixedLength = 8;

	public long CurrentByteOffset;

	public override FileInformationClass FileInformationClass => FileInformationClass.FilePositionInformation;

	public override int Length => 8;

	public FilePositionInformation()
	{
	}

	public FilePositionInformation(byte[] buffer, int offset)
	{
		CurrentByteOffset = LittleEndianConverter.ToInt64(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, CurrentByteOffset);
	}
}
