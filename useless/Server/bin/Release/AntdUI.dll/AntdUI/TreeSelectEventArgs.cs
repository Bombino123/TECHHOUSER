using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class TreeSelectEventArgs : VMEventArgs<TreeItem>
{
	public Rectangle Rect { get; private set; }

	public TreeSelectEventArgs(TreeItem item, Rectangle rect, MouseEventArgs e)
		: base(item, e)
	{
		Rect = rect;
	}
}
