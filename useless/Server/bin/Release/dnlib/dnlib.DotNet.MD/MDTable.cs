using System;
using System.Collections.Generic;
using System.Diagnostics;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[DebuggerDisplay("DL:{dataReader.Length} R:{numRows} RS:{tableInfo.RowSize} C:{Count} {tableInfo.Name}")]
public sealed class MDTable : IDisposable, IFileSection
{
	private readonly Table table;

	private uint numRows;

	private TableInfo tableInfo;

	private DataReader dataReader;

	internal readonly ColumnInfo Column0;

	internal readonly ColumnInfo Column1;

	internal readonly ColumnInfo Column2;

	internal readonly ColumnInfo Column3;

	internal readonly ColumnInfo Column4;

	internal readonly ColumnInfo Column5;

	internal readonly ColumnInfo Column6;

	internal readonly ColumnInfo Column7;

	internal readonly ColumnInfo Column8;

	private int Count => tableInfo.Columns.Length;

	public FileOffset StartOffset => (FileOffset)dataReader.StartOffset;

	public FileOffset EndOffset => (FileOffset)dataReader.EndOffset;

	public Table Table => table;

	public string Name => tableInfo.Name;

	public uint Rows => numRows;

	public uint RowSize => (uint)tableInfo.RowSize;

	public IList<ColumnInfo> Columns => tableInfo.Columns;

	public bool IsEmpty => numRows == 0;

	public TableInfo TableInfo => tableInfo;

	internal DataReader DataReader
	{
		get
		{
			return dataReader;
		}
		set
		{
			dataReader = value;
		}
	}

	internal MDTable(Table table, uint numRows, TableInfo tableInfo)
	{
		this.table = table;
		this.numRows = numRows;
		this.tableInfo = tableInfo;
		ColumnInfo[] columns = tableInfo.Columns;
		int num = columns.Length;
		if (num > 0)
		{
			Column0 = columns[0];
		}
		if (num > 1)
		{
			Column1 = columns[1];
		}
		if (num > 2)
		{
			Column2 = columns[2];
		}
		if (num > 3)
		{
			Column3 = columns[3];
		}
		if (num > 4)
		{
			Column4 = columns[4];
		}
		if (num > 5)
		{
			Column5 = columns[5];
		}
		if (num > 6)
		{
			Column6 = columns[6];
		}
		if (num > 7)
		{
			Column7 = columns[7];
		}
		if (num > 8)
		{
			Column8 = columns[8];
		}
	}

	public bool IsValidRID(uint rid)
	{
		if (rid != 0)
		{
			return rid <= numRows;
		}
		return false;
	}

	public bool IsInvalidRID(uint rid)
	{
		if (rid != 0)
		{
			return rid > numRows;
		}
		return true;
	}

	public void Dispose()
	{
		numRows = 0u;
		tableInfo = null;
		dataReader = default(DataReader);
	}
}
