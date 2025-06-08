using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class AceHeader
{
	public const int Length = 4;

	public AceType AceType;

	public AceFlags AceFlags;

	public ushort AceSize;

	public AceHeader()
	{
	}

	public AceHeader(byte[] buffer, int offset)
	{
		AceType = (AceType)ByteReader.ReadByte(buffer, offset);
		AceFlags = (AceFlags)ByteReader.ReadByte(buffer, offset + 1);
		AceSize = LittleEndianConverter.ToUInt16(buffer, offset + 2);
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		ByteWriter.WriteByte(buffer, ref offset, (byte)AceType);
		ByteWriter.WriteByte(buffer, ref offset, (byte)AceFlags);
		LittleEndianWriter.WriteUInt16(buffer, ref offset, AceSize);
	}
}
