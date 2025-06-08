using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public struct DataRepresentationFormat
{
	public CharacterFormat CharacterFormat;

	public ByteOrder ByteOrder;

	public FloatingPointRepresentation FloatingPointRepresentation;

	public DataRepresentationFormat(CharacterFormat characterFormat, ByteOrder byteOrder, FloatingPointRepresentation floatingPointRepresentation)
	{
		CharacterFormat = characterFormat;
		ByteOrder = byteOrder;
		FloatingPointRepresentation = floatingPointRepresentation;
	}

	public DataRepresentationFormat(byte[] buffer, int offset)
	{
		CharacterFormat = (CharacterFormat)(buffer[offset] & 0xFu);
		ByteOrder = (ByteOrder)(buffer[offset] >> 4);
		FloatingPointRepresentation = (FloatingPointRepresentation)buffer[offset + 1];
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		buffer[offset] = (byte)CharacterFormat;
		buffer[offset] |= (byte)((uint)ByteOrder << 4);
		buffer[offset + 1] = (byte)FloatingPointRepresentation;
	}
}
