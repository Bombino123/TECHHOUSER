using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class ResultList : List<ResultElement>
{
	public byte Reserved;

	public ushort Reserved2;

	public int Length => 4 + 24 * base.Count;

	public ResultList()
	{
	}

	public ResultList(byte[] buffer, int offset)
	{
		byte b = ByteReader.ReadByte(buffer, offset);
		Reserved = ByteReader.ReadByte(buffer, offset + 1);
		Reserved2 = LittleEndianConverter.ToUInt16(buffer, offset + 2);
		offset += 4;
		for (int i = 0; i < b; i++)
		{
			ResultElement item = new ResultElement(buffer, offset);
			Add(item);
			offset += 24;
		}
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		byte b = (byte)base.Count;
		ByteWriter.WriteByte(buffer, offset, b);
		ByteWriter.WriteByte(buffer, offset + 1, Reserved);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved2);
		offset += 4;
		for (int i = 0; i < b; i++)
		{
			base[i].WriteBytes(buffer, offset);
			offset += 24;
		}
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		WriteBytes(buffer, offset);
		offset += Length;
	}
}
