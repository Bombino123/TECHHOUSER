using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class PipeWaitRequest
{
	public const int FixedLength = 14;

	public ulong Timeout;

	private uint NameLength;

	public bool TimeSpecified;

	public byte Padding;

	public string Name;

	public int Length => 14 + Name.Length * 2;

	public PipeWaitRequest()
	{
	}

	public PipeWaitRequest(byte[] buffer, int offset)
	{
		Timeout = LittleEndianConverter.ToUInt64(buffer, offset);
		NameLength = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		TimeSpecified = Convert.ToBoolean(ByteReader.ReadByte(buffer, offset + 12));
		Padding = ByteReader.ReadByte(buffer, offset + 13);
		Name = ByteReader.ReadUTF16String(buffer, offset + 14, (int)(NameLength / 2));
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		LittleEndianWriter.WriteUInt64(array, 0, Timeout);
		LittleEndianWriter.WriteUInt32(array, 8, (uint)(Name.Length * 2));
		ByteWriter.WriteByte(array, 12, Convert.ToByte(TimeSpecified));
		ByteWriter.WriteByte(array, 13, Padding);
		ByteWriter.WriteUTF16String(array, 14, Name);
		return array;
	}
}
