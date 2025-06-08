using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileStreamEntry
{
	public const int FixedLength = 24;

	public uint NextEntryOffset;

	private uint StreamNameLength;

	public long StreamSize;

	public long StreamAllocationSize;

	public string StreamName = string.Empty;

	public int Length => 24 + StreamName.Length * 2;

	public int PaddedLength
	{
		get
		{
			int length = Length;
			int num = (8 - length % 8) % 8;
			return length + num;
		}
	}

	public FileStreamEntry()
	{
	}

	public FileStreamEntry(byte[] buffer, int offset)
	{
		NextEntryOffset = LittleEndianConverter.ToUInt32(buffer, offset);
		StreamNameLength = LittleEndianConverter.ToUInt32(buffer, offset + 4);
		StreamSize = LittleEndianConverter.ToInt64(buffer, offset + 8);
		StreamAllocationSize = LittleEndianConverter.ToInt64(buffer, offset + 16);
		StreamName = ByteReader.ReadUTF16String(buffer, offset + 24, (int)StreamNameLength / 2);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		StreamNameLength = (uint)(StreamName.Length * 2);
		LittleEndianWriter.WriteUInt32(buffer, offset, NextEntryOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, StreamNameLength);
		LittleEndianWriter.WriteInt64(buffer, offset + 8, StreamSize);
		LittleEndianWriter.WriteInt64(buffer, offset + 16, StreamAllocationSize);
		ByteWriter.WriteUTF16String(buffer, offset + 24, StreamName);
	}
}
