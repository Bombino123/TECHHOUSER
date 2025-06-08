namespace AntdUI;

public class TableEventArgs : ITableEventArgs
{
	public object? Value { get; private set; }

	public TableEventArgs(object? value, object? record, int rowIndex, int columnIndex)
		: base(record, rowIndex, columnIndex)
	{
		Value = value;
	}
}
