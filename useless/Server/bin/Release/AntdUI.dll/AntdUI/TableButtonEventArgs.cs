using System.Windows.Forms;

namespace AntdUI;

public class TableButtonEventArgs : MouseEventArgs
{
	public CellLink Btn { get; private set; }

	public object? Record { get; private set; }

	public int RowIndex { get; private set; }

	public int ColumnIndex { get; private set; }

	public TableButtonEventArgs(CellLink btn, object? record, int rowIndex, int columnIndex, MouseEventArgs e)
		: base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Btn = btn;
		Record = record;
		RowIndex = rowIndex;
		ColumnIndex = columnIndex;
	}
}
