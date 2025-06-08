using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public struct FileID
{
	public const int Length = 16;

	public ulong Persistent;

	public ulong Volatile;

	public FileID(byte[] buffer, int offset)
	{
		Persistent = LittleEndianConverter.ToUInt64(buffer, offset);
		Volatile = LittleEndianConverter.ToUInt64(buffer, offset + 8);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt64(buffer, offset, Persistent);
		LittleEndianWriter.WriteUInt64(buffer, offset + 8, Volatile);
	}
}
