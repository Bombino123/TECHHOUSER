using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class VirtualPanelMouseArgs : MouseEventArgs
{
	public VirtualItem Item { get; private set; }

	public Rectangle Rect { get; private set; }

	public VirtualPanelMouseArgs(VirtualItem item, Rectangle rect, int x, int y, MouseEventArgs e)
		: base(e.Button, e.Clicks, x - rect.X, y - rect.Y, e.Delta)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Item = item;
		Rect = rect;
	}

	public VirtualPanelMouseArgs(VirtualItem item, Rectangle rect, int x, int y, MouseEventArgs e, int doubleclick)
		: base(e.Button, doubleclick, x - rect.X, y - rect.Y, e.Delta)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Item = item;
		Rect = rect;
	}
}
