using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class ExtendedAttributeName
{
	private byte AttributeNameLengthInBytes;

	public string AttributeName;

	public int Length => 1 + AttributeName.Length + 1;

	public ExtendedAttributeName()
	{
	}

	public ExtendedAttributeName(byte[] buffer, int offset)
	{
		AttributeNameLengthInBytes = ByteReader.ReadByte(buffer, offset);
		AttributeName = ByteReader.ReadAnsiString(buffer, offset + 1, AttributeNameLengthInBytes);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		AttributeNameLengthInBytes = (byte)AttributeName.Length;
		ByteWriter.WriteByte(buffer, offset, AttributeNameLengthInBytes);
		ByteWriter.WriteAnsiString(buffer, offset + 1, AttributeName, AttributeName.Length);
	}
}
