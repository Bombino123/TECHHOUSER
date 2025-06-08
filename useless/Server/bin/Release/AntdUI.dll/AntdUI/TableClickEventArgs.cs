using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class TableClickEventArgs : MouseEventArgs
{
	public object? Record { get; private set; }

	public int RowIndex { get; private set; }

	public int ColumnIndex { get; private set; }

	public Rectangle Rect { get; private set; }

	public TableClickEventArgs(object? record, int rowIndex, int columnIndex, Rectangle rect, MouseEventArgs e)
		: base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Record = record;
		RowIndex = rowIndex;
		ColumnIndex = columnIndex;
		Rect = rect;
	}
}
