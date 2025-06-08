using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[DebuggerDisplay("{rowSize} {name}")]
[ComVisible(true)]
public sealed class TableInfo
{
	private readonly Table table;

	private int rowSize;

	private readonly ColumnInfo[] columns;

	private readonly string name;

	public Table Table => table;

	public int RowSize
	{
		get
		{
			return rowSize;
		}
		internal set
		{
			rowSize = value;
		}
	}

	public ColumnInfo[] Columns => columns;

	public string Name => name;

	public TableInfo(Table table, string name, ColumnInfo[] columns)
	{
		this.table = table;
		this.name = name;
		this.columns = columns;
	}

	public TableInfo(Table table, string name, ColumnInfo[] columns, int rowSize)
	{
		this.table = table;
		this.name = name;
		this.columns = columns;
		this.rowSize = rowSize;
	}
}
