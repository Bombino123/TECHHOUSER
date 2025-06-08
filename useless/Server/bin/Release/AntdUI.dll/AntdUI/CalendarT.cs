using System.Drawing;

namespace AntdUI;

internal class CalendarT
{
	public bool hover { get; set; }

	public Rectangle rect { get; set; }

	public Rectangle rect_read { get; set; }

	public int x { get; set; }

	public int y { get; set; }

	public int t { get; set; }

	public string v { get; set; }

	public CalendarT(int _x, int _y, int _t)
	{
		x = _x;
		y = _y;
		t = _t;
		v = _t.ToString().PadLeft(2, '0');
	}

	internal bool Contains(Point point, float x, float y, out bool change)
	{
		if (rect.Contains(point.X + (int)x, point.Y + (int)y))
		{
			change = SetHover(val: true);
			return true;
		}
		change = SetHover(val: false);
		return false;
	}

	internal bool SetHover(bool val)
	{
		bool result = false;
		if (val)
		{
			if (!hover)
			{
				result = true;
			}
			hover = true;
		}
		else
		{
			if (hover)
			{
				result = true;
			}
			hover = false;
		}
		return result;
	}
}
