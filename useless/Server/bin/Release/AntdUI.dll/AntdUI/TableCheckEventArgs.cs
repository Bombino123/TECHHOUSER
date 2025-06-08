namespace AntdUI;

public class TableCheckEventArgs : ITableEventArgs
{
	public bool Value { get; private set; }

	public TableCheckEventArgs(bool value, object? record, int rowIndex, int columnIndex)
		: base(record, rowIndex, columnIndex)
	{
		Value = value;
	}
}
