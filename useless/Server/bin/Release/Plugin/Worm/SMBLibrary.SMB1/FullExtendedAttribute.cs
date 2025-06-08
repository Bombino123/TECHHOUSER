using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FullExtendedAttribute
{
	public ExtendedAttributeFlags ExtendedAttributeFlag;

	private byte AttributeNameLengthInBytes;

	private ushort AttributeValueLengthInBytes;

	public string AttributeName;

	public string AttributeValue;

	public int Length => 4 + AttributeName.Length + 1 + AttributeValue.Length;

	public FullExtendedAttribute()
	{
	}

	public FullExtendedAttribute(byte[] buffer, int offset)
	{
		ExtendedAttributeFlag = (ExtendedAttributeFlags)ByteReader.ReadByte(buffer, offset);
		AttributeNameLengthInBytes = ByteReader.ReadByte(buffer, offset + 1);
		AttributeValueLengthInBytes = LittleEndianConverter.ToUInt16(buffer, offset + 2);
		AttributeName = ByteReader.ReadAnsiString(buffer, offset + 4, AttributeNameLengthInBytes);
		AttributeValue = ByteReader.ReadAnsiString(buffer, offset + 4 + AttributeNameLengthInBytes + 1, AttributeValueLengthInBytes);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		AttributeNameLengthInBytes = (byte)AttributeName.Length;
		AttributeValueLengthInBytes = (ushort)AttributeValue.Length;
		ByteWriter.WriteByte(buffer, offset, (byte)ExtendedAttributeFlag);
		ByteWriter.WriteByte(buffer, offset + 1, AttributeNameLengthInBytes);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, AttributeValueLengthInBytes);
		ByteWriter.WriteAnsiString(buffer, offset + 4, AttributeName, AttributeName.Length);
		ByteWriter.WriteAnsiString(buffer, offset + 4 + AttributeNameLengthInBytes + 1, AttributeValue, AttributeValue.Length);
	}
}
