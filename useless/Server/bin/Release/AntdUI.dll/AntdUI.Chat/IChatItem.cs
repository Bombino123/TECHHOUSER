using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI.Chat;

public class IChatItem
{
	[Description("用户定义数据")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	internal bool show { get; set; }

	internal bool Show { get; set; }

	internal ChatList? PARENT { get; set; }

	internal Rectangle rect { get; set; }

	internal void Invalidate()
	{
		ChatList? pARENT = PARENT;
		if (pARENT != null)
		{
			((Control)pARENT).Invalidate();
		}
	}

	internal void Invalidates()
	{
		if (PARENT != null)
		{
			PARENT.ChangeList();
			((Control)PARENT).Invalidate();
		}
	}

	internal bool Contains(Point point, int x, int y)
	{
		return rect.Contains(new Point(point.X + x, point.Y + y));
	}
}
