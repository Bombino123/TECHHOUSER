using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileInternalInformation : FileInformation
{
	public const int FixedLength = 8;

	public long IndexNumber;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileInternalInformation;

	public override int Length => 8;

	public FileInternalInformation()
	{
	}

	public FileInternalInformation(byte[] buffer, int offset)
	{
		IndexNumber = LittleEndianConverter.ToInt64(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, IndexNumber);
	}
}
