namespace AntdUI;

public class TableBeginEditInputStyleEventArgs : ITableEventArgs
{
	public object? Value { get; private set; }

	public Input Input { get; private set; }

	public TableBeginEditInputStyleEventArgs(object? value, object? record, int rowIndex, int columnIndex, ref Input input)
		: base(record, rowIndex, columnIndex)
	{
		Value = value;
		Input = input;
	}
}
