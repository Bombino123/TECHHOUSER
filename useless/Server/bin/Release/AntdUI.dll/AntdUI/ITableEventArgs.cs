using System;

namespace AntdUI;

public class ITableEventArgs : EventArgs
{
	public object? Record { get; private set; }

	public int RowIndex { get; private set; }

	public int ColumnIndex { get; private set; }

	public ITableEventArgs(object? record, int rowIndex, int columnIndex)
	{
		Record = record;
		RowIndex = rowIndex;
		ColumnIndex = columnIndex;
	}
}
