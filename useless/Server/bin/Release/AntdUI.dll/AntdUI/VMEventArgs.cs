using System.Windows.Forms;

namespace AntdUI;

public class VMEventArgs<T> : MouseEventArgs
{
	public T Item { get; private set; }

	public VMEventArgs(T item, MouseEventArgs e)
		: base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Item = item;
	}
}
