using System;

namespace AntdUI;

public class TableSetRowStyleEventArgs : EventArgs
{
	public object? Record { get; private set; }

	public int RowIndex { get; private set; }

	public TableSetRowStyleEventArgs(object? record, int rowIndex)
	{
		Record = record;
		RowIndex = rowIndex;
	}
}
