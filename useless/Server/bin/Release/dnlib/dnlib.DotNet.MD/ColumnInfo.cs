using System;
using System.Diagnostics;
using dnlib.DotNet.Writer;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[DebuggerDisplay("{offset} {size} {name}")]
public sealed class ColumnInfo
{
	private readonly byte index;

	private byte offset;

	private readonly ColumnSize columnSize;

	private byte size;

	private readonly string name;

	public int Index => index;

	public int Offset
	{
		get
		{
			return offset;
		}
		internal set
		{
			offset = (byte)value;
		}
	}

	public int Size
	{
		get
		{
			return size;
		}
		internal set
		{
			size = (byte)value;
		}
	}

	public string Name => name;

	public ColumnSize ColumnSize => columnSize;

	public ColumnInfo(byte index, string name, ColumnSize columnSize)
	{
		this.index = index;
		this.name = name;
		this.columnSize = columnSize;
	}

	public ColumnInfo(byte index, string name, ColumnSize columnSize, byte offset, byte size)
	{
		this.index = index;
		this.name = name;
		this.columnSize = columnSize;
		this.offset = offset;
		this.size = size;
	}

	public uint Read(ref DataReader reader)
	{
		return size switch
		{
			1 => reader.ReadByte(), 
			2 => reader.ReadUInt16(), 
			4 => reader.ReadUInt32(), 
			_ => throw new InvalidOperationException("Invalid column size"), 
		};
	}

	internal uint Unsafe_Read24(ref DataReader reader)
	{
		if (size != 2)
		{
			return reader.Unsafe_ReadUInt32();
		}
		return reader.Unsafe_ReadUInt16();
	}

	public void Write(DataWriter writer, uint value)
	{
		switch (size)
		{
		case 1:
			writer.WriteByte((byte)value);
			break;
		case 2:
			writer.WriteUInt16((ushort)value);
			break;
		case 4:
			writer.WriteUInt32(value);
			break;
		default:
			throw new InvalidOperationException("Invalid column size");
		}
	}

	internal void Write24(DataWriter writer, uint value)
	{
		if (size == 2)
		{
			writer.WriteUInt16((ushort)value);
		}
		else
		{
			writer.WriteUInt32(value);
		}
	}
}
