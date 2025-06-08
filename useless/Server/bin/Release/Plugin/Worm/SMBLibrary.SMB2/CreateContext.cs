using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class CreateContext
{
	public const int FixedLength = 16;

	public uint Next;

	private ushort NameOffset;

	private ushort NameLength;

	public ushort Reserved;

	private ushort DataOffset;

	private uint DataLength;

	public string Name = string.Empty;

	public byte[] Data = new byte[0];

	public int Length
	{
		get
		{
			if (Data.Length != 0)
			{
				int num = (int)Math.Ceiling((double)(Name.Length * 2) / 8.0) * 8;
				return 16 + num + Data.Length;
			}
			return 16 + Name.Length * 2;
		}
	}

	public CreateContext()
	{
	}

	public CreateContext(byte[] buffer, int offset)
	{
		Next = LittleEndianConverter.ToUInt32(buffer, offset);
		NameOffset = LittleEndianConverter.ToUInt16(buffer, offset + 4);
		NameLength = LittleEndianConverter.ToUInt16(buffer, offset + 6);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 8);
		DataOffset = LittleEndianConverter.ToUInt16(buffer, offset + 10);
		DataLength = LittleEndianConverter.ToUInt32(buffer, offset + 12);
		if (NameLength > 0)
		{
			Name = ByteReader.ReadUTF16String(buffer, offset + NameOffset, NameLength / 2);
		}
		if (DataLength != 0)
		{
			Data = ByteReader.ReadBytes(buffer, offset + DataOffset, (int)DataLength);
		}
	}

	private void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, offset, Next);
		NameOffset = 0;
		NameLength = (ushort)(Name.Length * 2);
		if (Name.Length > 0)
		{
			NameOffset = 16;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset + 4, NameOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 6, NameLength);
		LittleEndianWriter.WriteUInt16(buffer, offset + 8, Reserved);
		DataOffset = 0;
		DataLength = (uint)Data.Length;
		if (Data.Length != 0)
		{
			int num = (int)Math.Ceiling((double)(Name.Length * 2) / 8.0) * 8;
			DataOffset = (ushort)(16 + num);
		}
		LittleEndianWriter.WriteUInt16(buffer, offset + 10, DataOffset);
		ByteWriter.WriteUTF16String(buffer, NameOffset, Name);
		ByteWriter.WriteBytes(buffer, DataOffset, Data);
	}

	public static List<CreateContext> ReadCreateContextList(byte[] buffer, int offset)
	{
		List<CreateContext> list = new List<CreateContext>();
		CreateContext createContext;
		do
		{
			createContext = new CreateContext(buffer, offset);
			list.Add(createContext);
			offset += (int)createContext.Next;
		}
		while (createContext.Next != 0);
		return list;
	}

	public static void WriteCreateContextList(byte[] buffer, int offset, List<CreateContext> createContexts)
	{
		for (int i = 0; i < createContexts.Count; i++)
		{
			CreateContext createContext = createContexts[i];
			int num = (int)Math.Ceiling((double)createContext.Length / 8.0) * 8;
			if (i < createContexts.Count - 1)
			{
				createContext.Next = (uint)num;
			}
			else
			{
				createContext.Next = 0u;
			}
			createContext.WriteBytes(buffer, offset);
			offset += num;
		}
	}

	public static int GetCreateContextListLength(List<CreateContext> createContexts)
	{
		int num = 0;
		for (int i = 0; i < createContexts.Count; i++)
		{
			int length = createContexts[i].Length;
			if (i < createContexts.Count - 1)
			{
				int num2 = (int)Math.Ceiling((double)length / 8.0) * 8;
				num += num2;
			}
			else
			{
				num += length;
			}
		}
		return num;
	}
}
