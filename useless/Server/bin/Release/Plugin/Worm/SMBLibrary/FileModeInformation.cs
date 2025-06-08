using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileModeInformation : FileInformation
{
	public const int FixedSize = 4;

	public CreateOptions FileMode;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileModeInformation;

	public override int Length => 4;

	public FileModeInformation()
	{
	}

	public FileModeInformation(byte[] buffer, int offset)
	{
		FileMode = (CreateOptions)LittleEndianConverter.ToUInt32(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, offset, (uint)FileMode);
	}
}
