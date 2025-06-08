using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileEaInformation : FileInformation
{
	public const int FixedLength = 4;

	public uint EaSize;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileEaInformation;

	public override int Length => 4;

	public FileEaInformation()
	{
	}

	public FileEaInformation(byte[] buffer, int offset)
	{
		EaSize = LittleEndianConverter.ToUInt32(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, offset, EaSize);
	}
}
