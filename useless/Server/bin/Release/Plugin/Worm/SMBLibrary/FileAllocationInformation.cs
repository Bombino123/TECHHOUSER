using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileAllocationInformation : FileInformation
{
	public const int FixedLength = 8;

	public long AllocationSize;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileAllocationInformation;

	public override int Length => 8;

	public FileAllocationInformation()
	{
	}

	public FileAllocationInformation(byte[] buffer, int offset)
	{
		AllocationSize = LittleEndianConverter.ToInt64(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteInt64(buffer, offset, AllocationSize);
	}
}
