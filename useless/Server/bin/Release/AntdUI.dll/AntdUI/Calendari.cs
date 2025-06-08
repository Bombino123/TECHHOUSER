using System;
using System.Drawing;

namespace AntdUI;

internal class Calendari
{
	private Rectangle _rect;

	public Rectangle rect_read;

	public Rectangle rect_f;

	public Rectangle rect_l;

	public bool hover { get; set; }

	public Rectangle rect
	{
		get
		{
			return _rect;
		}
		set
		{
			rect_read = new Rectangle(value.X + 4, value.Y + 4, value.Width - 8, value.Height - 8);
			_rect = value;
		}
	}

	public int x { get; set; }

	public int y { get; set; }

	public int t { get; set; }

	public string v { get; set; }

	public DateTime date { get; set; }

	public string date_str { get; set; }

	public bool enable { get; set; }

	public Calendari(int _t, int _x, int _y, string _v, DateTime _date, string str, DateTime? min, DateTime? max)
	{
		t = _t;
		x = _x;
		y = _y;
		v = _v;
		date = _date;
		date_str = str;
		enable = Helper.DateExceedRelax(_date, min, max);
	}

	public Calendari(int _t, int _x, int _y, string _v, DateTime _date, DateTime? min, DateTime? max)
	{
		t = _t;
		x = _x;
		y = _y;
		v = _v;
		date = _date;
		date_str = _date.ToString("yyyy-MM-dd");
		enable = Helper.DateExceed(_date, min, max);
	}

	internal void SetRect(Rectangle value, int gap)
	{
		int num = (value.Width - gap) / 2;
		rect_read = new Rectangle(value.X + num, value.Y + num, gap, gap);
		_rect = value;
	}

	internal void SetRectG(Rectangle value, float gap)
	{
		int num = (int)((float)value.Width * gap);
		int num2 = (int)((float)value.Height * gap);
		rect_read = new Rectangle(value.X + (value.Width - num) / 2, value.Y + (value.Height - num2) / 2, num, num2);
		_rect = value;
	}
}
