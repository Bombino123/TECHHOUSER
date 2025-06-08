using System.Drawing;

namespace AntdUI;

internal class CalendarButton
{
	public bool hover { get; set; }

	public Rectangle rect { get; set; }

	public Rectangle rect_read { get; set; }

	public Rectangle rect_text { get; set; }

	public int y { get; set; }

	public string v { get; set; }

	public object Tag { get; set; }

	public CalendarButton(int _y, object _v)
	{
		y = _y;
		v = _v.ToString();
		Tag = _v;
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
