using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public struct Version
{
	public const int Length = 2;

	public byte Major;

	public byte Minor;

	public Version(byte[] buffer, int offset)
	{
		Major = ByteReader.ReadByte(buffer, offset);
		Minor = ByteReader.ReadByte(buffer, offset + 1);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		ByteWriter.WriteByte(buffer, offset, Major);
		ByteWriter.WriteByte(buffer, offset + 1, Minor);
	}
}
