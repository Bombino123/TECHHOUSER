using System.Collections.Generic;
using System.Drawing;

namespace AntdUI;

internal class ObjectItem
{
	public object Val { get; set; }

	public bool Enable { get; set; } = true;


	public IList<object>? Sub { get; set; }

	internal bool has_sub { get; set; }

	public int? Online { get; set; }

	public Color? OnlineCustom { get; set; }

	public Image? Icon { get; set; }

	public string? IconSvg { get; set; }

	public bool HasIcon
	{
		get
		{
			if (IconSvg == null)
			{
				return Icon != null;
			}
			return true;
		}
	}

	public Rectangle RectIcon { get; set; }

	public Rectangle RectOnline { get; set; }

	public string? SubText { get; set; }

	public string Text { get; set; }

	private string[] PY { get; set; }

	public int ID { get; set; }

	public bool Hover { get; set; }

	public bool Show { get; set; }

	internal bool Group { get; set; }

	internal bool NoIndex { get; set; }

	internal bool ShowAndID
	{
		get
		{
			if (ID != -1)
			{
				return !Show;
			}
			return true;
		}
	}

	internal Rectangle RectArrow { get; set; }

	public Rectangle Rect { get; set; }

	public Rectangle RectText { get; set; }

	public ObjectItem(object _val, int _i, Rectangle rect, Rectangle rect_text)
	{
		Show = true;
		Val = _val;
		Text = _val.ToString() ?? string.Empty;
		ID = _i;
		SetRect(rect, rect_text);
		string text = Text;
		PY = new string[3]
		{
			text.ToLower(),
			Pinyin.GetPinyin(text).ToLower(),
			Pinyin.GetInitials(text).ToLower()
		};
	}

	public ObjectItem(GroupSelectItem _val, int _i, Rectangle rect, Rectangle rect_text)
	{
		Show = (Group = true);
		Val = _val;
		Text = _val.Title;
		ID = _i;
		SetRect(rect, rect_text);
		string text = Text;
		PY = new string[3]
		{
			text.ToLower(),
			Pinyin.GetPinyin(text).ToLower(),
			Pinyin.GetInitials(text).ToLower()
		};
	}

	public ObjectItem(SelectItem _val, int _i, Rectangle rect, Rectangle rect_text, int gap_x, int gap_x2, int gap_y, int gap_y2)
	{
		Sub = _val.Sub;
		if (Sub != null && Sub.Count > 0)
		{
			has_sub = true;
		}
		Show = true;
		Val = _val;
		Online = _val.Online;
		OnlineCustom = _val.OnlineCustom;
		Icon = _val.Icon;
		IconSvg = _val.IconSvg;
		Text = _val.Text;
		SubText = _val.SubText;
		Enable = _val.Enable;
		ID = _i;
		SetRect(rect, rect_text, gap_x, gap_x2, gap_y, gap_y2);
		string text = _val.Text + _val.SubText;
		PY = new string[3]
		{
			text.ToLower(),
			Pinyin.GetPinyin(text).ToLower(),
			Pinyin.GetInitials(text).ToLower()
		};
	}

	public ObjectItem(Rectangle rect)
	{
		ID = -1;
		Rect = rect;
		Show = true;
		PY = new string[0];
	}

	public int Contains(string val, out bool select)
	{
		select = false;
		int num = PY.Length;
		int num2 = 0;
		if (Text == val)
		{
			select = true;
			num2 += num * 10;
		}
		val = val.ToLower();
		if (Text == val)
		{
			select = true;
			num2 += num * 8;
		}
		string[] pY = PY;
		foreach (string text in pY)
		{
			if (text == val)
			{
				select = true;
				num2 += num * 3;
			}
			else if (text.StartsWith(val))
			{
				num2 += num * 2;
			}
			else if (text.Contains(val))
			{
				num2 += num;
			}
			num--;
		}
		return num2;
	}

	internal void SetRect(Rectangle rect, Rectangle rect_text, int gap_x, int gap_x2, int gap_y, int gap_y2)
	{
		Rect = rect;
		if (Val is SelectItem)
		{
			if (Online > -1 || HasIcon)
			{
				if (Online > -1 && HasIcon)
				{
					RectOnline = new Rectangle(rect_text.X - gap_y / 2, rect_text.Y + (rect_text.Height - gap_y) / 2, gap_y, gap_y);
					RectIcon = new Rectangle(rect_text.X + gap_y2, rect_text.Y, rect_text.Height, rect_text.Height);
					RectText = new Rectangle(rect_text.X + gap_y + gap_y2 + rect_text.Height, rect_text.Y, rect_text.Width - rect_text.Height - gap_y - gap_y2, rect_text.Height);
				}
				else if (Online > -1)
				{
					RectOnline = new Rectangle(rect_text.X - gap_y / 2, rect_text.Y + (rect_text.Height - gap_y) / 2, gap_y, gap_y);
					RectText = new Rectangle(rect_text.X + gap_y2, rect_text.Y, rect_text.Width - gap_y2, rect_text.Height);
				}
				else
				{
					RectIcon = new Rectangle(rect.X + gap_x / 2, rect_text.Y, rect_text.Height, rect_text.Height);
					RectText = new Rectangle(rect_text.X + rect_text.Height, rect_text.Y, rect_text.Width - rect_text.Height, rect_text.Height);
				}
			}
			else
			{
				RectText = rect_text;
			}
			RectArrow = new Rectangle(Rect.Right - Rect.Height - gap_y, Rect.Y, Rect.Height, Rect.Height);
		}
		else
		{
			RectText = rect_text;
		}
	}

	internal void SetRect(Rectangle rect, Rectangle rect_text)
	{
		Rect = rect;
		RectText = rect_text;
	}

	internal bool SetHover(bool val)
	{
		bool result = false;
		if (val)
		{
			if (!Hover)
			{
				result = true;
			}
			Hover = true;
		}
		else
		{
			if (Hover)
			{
				result = true;
			}
			Hover = false;
		}
		return result;
	}

	internal bool Contains(Point point, int x, int y, out bool change)
	{
		if (ID > -1 && Rect.Contains(point.X + x, point.Y + y))
		{
			change = SetHover(val: true);
			return true;
		}
		change = SetHover(val: false);
		return false;
	}
}
