using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class ContextList : List<ContextElement>
{
	public byte Reserved1;

	public ushort Reserved2;

	public int Length
	{
		get
		{
			int num = 4;
			for (int i = 0; i < base.Count; i++)
			{
				num += base[i].Length;
			}
			return num;
		}
	}

	public ContextList()
	{
	}

	public ContextList(byte[] buffer, int offset)
	{
		byte b = ByteReader.ReadByte(buffer, offset);
		Reserved1 = ByteReader.ReadByte(buffer, offset + 1);
		Reserved2 = LittleEndianConverter.ToUInt16(buffer, offset + 2);
		offset += 4;
		for (int i = 0; i < b; i++)
		{
			ContextElement contextElement = new ContextElement(buffer, offset);
			Add(contextElement);
			offset += contextElement.Length;
		}
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		byte b = (byte)base.Count;
		ByteWriter.WriteByte(buffer, offset, b);
		ByteWriter.WriteByte(buffer, offset + 1, Reserved1);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved2);
		offset += 4;
		for (int i = 0; i < b; i++)
		{
			base[i].WriteBytes(buffer, offset);
			offset += base[i].Length;
		}
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		WriteBytes(buffer, offset);
		offset += Length;
	}
}
