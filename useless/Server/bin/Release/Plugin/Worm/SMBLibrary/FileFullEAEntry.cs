using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFullEAEntry
{
	public const int FixedLength = 8;

	public uint NextEntryOffset;

	public ExtendedAttributeFlags Flags;

	private byte EaNameLength;

	private ushort EaValueLength;

	public string EaName;

	public string EaValue;

	public int Length => 8 + EaName.Length + 1 + EaValue.Length;

	public FileFullEAEntry()
	{
	}

	public FileFullEAEntry(byte[] buffer, int offset)
	{
		NextEntryOffset = LittleEndianReader.ReadUInt32(buffer, ref offset);
		Flags = (ExtendedAttributeFlags)ByteReader.ReadByte(buffer, ref offset);
		EaNameLength = ByteReader.ReadByte(buffer, ref offset);
		EaValueLength = LittleEndianReader.ReadUInt16(buffer, ref offset);
		EaName = ByteReader.ReadAnsiString(buffer, ref offset, EaNameLength);
		offset++;
		EaValue = ByteReader.ReadAnsiString(buffer, ref offset, EaValueLength);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		EaNameLength = (byte)EaName.Length;
		EaValueLength = (ushort)EaValue.Length;
		LittleEndianWriter.WriteUInt32(buffer, ref offset, NextEntryOffset);
		ByteWriter.WriteByte(buffer, ref offset, (byte)Flags);
		ByteWriter.WriteByte(buffer, ref offset, EaNameLength);
		LittleEndianWriter.WriteUInt16(buffer, ref offset, EaValueLength);
		ByteWriter.WriteAnsiString(buffer, ref offset, EaName);
		ByteWriter.WriteByte(buffer, ref offset, 0);
		ByteWriter.WriteAnsiString(buffer, ref offset, EaValue);
	}
}
