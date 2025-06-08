using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SetFileEndOfFileInfo : SetInformation
{
	public const int Length = 8;

	public long EndOfFile;

	public override SetInformationLevel InformationLevel => SetInformationLevel.SMB_SET_FILE_END_OF_FILE_INFO;

	public SetFileEndOfFileInfo()
	{
	}

	public SetFileEndOfFileInfo(byte[] buffer)
		: this(buffer, 0)
	{
	}

	public SetFileEndOfFileInfo(byte[] buffer, int offset)
	{
		EndOfFile = LittleEndianConverter.ToInt64(buffer, offset);
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[8];
		LittleEndianWriter.WriteInt64(array, 0, EndOfFile);
		return array;
	}
}
