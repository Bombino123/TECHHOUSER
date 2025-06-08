using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileAccessInformation : FileInformation
{
	public const int FixedLength = 4;

	public AccessMask AccessFlags;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileAccessInformation;

	public override int Length => 4;

	public FileAccessInformation()
	{
	}

	public FileAccessInformation(byte[] buffer, int offset)
	{
		AccessFlags = (AccessMask)LittleEndianConverter.ToUInt32(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, offset, (uint)AccessFlags);
	}
}
