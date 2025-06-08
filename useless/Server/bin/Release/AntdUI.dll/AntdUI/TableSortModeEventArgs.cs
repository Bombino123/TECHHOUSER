using System;

namespace AntdUI;

public class TableSortModeEventArgs : EventArgs
{
	public SortMode SortMode { get; private set; }

	public Column Column { get; private set; }

	public TableSortModeEventArgs(SortMode sortMode, Column column)
	{
		SortMode = sortMode;
		Column = column;
	}
}
