namespace AntdUI;

public class TableEndEditEventArgs : ITableEventArgs
{
	public string Value { get; private set; }

	public TableEndEditEventArgs(string value, object? record, int rowIndex, int columnIndex)
		: base(record, rowIndex, columnIndex)
	{
		Value = value;
	}
}
