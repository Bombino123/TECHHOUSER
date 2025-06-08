using System;
using System.Drawing;

namespace AntdUI;

public class TreeHoverEventArgs : EventArgs
{
	public TreeItem Item { get; private set; }

	public Rectangle Rect { get; private set; }

	public bool Hover { get; private set; }

	public TreeHoverEventArgs(TreeItem item, Rectangle rect, bool hover)
	{
		Item = item;
		Hover = hover;
		Rect = rect;
	}
}
