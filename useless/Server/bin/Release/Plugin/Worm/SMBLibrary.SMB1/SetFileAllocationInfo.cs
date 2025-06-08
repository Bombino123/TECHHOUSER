using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SetFileAllocationInfo : SetInformation
{
	public const int Length = 8;

	public long AllocationSize;

	public override SetInformationLevel InformationLevel => SetInformationLevel.SMB_SET_FILE_ALLOCATION_INFO;

	public SetFileAllocationInfo()
	{
	}

	public SetFileAllocationInfo(byte[] buffer)
		: this(buffer, 0)
	{
	}

	public SetFileAllocationInfo(byte[] buffer, int offset)
	{
		AllocationSize = LittleEndianConverter.ToInt64(buffer, offset);
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[8];
		LittleEndianWriter.WriteInt64(array, 0, AllocationSize);
		return array;
	}
}
