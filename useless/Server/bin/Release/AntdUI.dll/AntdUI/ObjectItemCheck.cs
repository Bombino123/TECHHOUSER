using System.Collections.Generic;
using System.Drawing;

namespace AntdUI;

internal class ObjectItemCheck
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

	public Rectangle RectCheck { get; set; }

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

	public Rectangle Rect { get; set; }

	public Rectangle RectText { get; set; }

	public Rectangle RectArrow { get; set; }

	public ObjectItemCheck(object _val, int _i, Rectangle rect, Rectangle rect_text, int gap_x, int gap_x2, int gap_y, int gap_y2)
	{
		Show = true;
		Val = _val;
		Text = _val.ToString() ?? string.Empty;
		ID = _i;
		SetRect(rect, rect_text, gap_x, gap_x2, gap_y, gap_y2);
		string text = Text;
		PY = new string[3]
		{
			text.ToLower(),
			Pinyin.GetPinyin(text).ToLower(),
			Pinyin.GetInitials(text).ToLower()
		};
	}

	public ObjectItemCheck(GroupSelectItem _val, int _i, Rectangle rect, Rectangle rect_text, int gap_x, int gap_x2, int gap_y, int gap_y2)
	{
		Show = (Group = true);
		Val = _val;
		Text = _val.Title;
		ID = _i;
		SetRect(rect, rect_text, gap_x, gap_x2, gap_y, gap_y2);
		string text = Text;
		PY = new string[3]
		{
			text.ToLower(),
			Pinyin.GetPinyin(text).ToLower(),
			Pinyin.GetInitials(text).ToLower()
		};
	}

	public ObjectItemCheck(SelectItem _val, int _i, Rectangle rect, Rectangle rect_text, int gap_x, int gap_x2, int gap_y, int gap_y2)
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

	public ObjectItemCheck(Rectangle rect)
	{
		ID = -1;
		Rect = rect;
		Show = true;
		PY = new string[0];
	}

	public bool Contains(string val)
	{
		string[] pY = PY;
		for (int i = 0; i < pY.Length; i++)
		{
			if (pY[i].Contains(val))
			{
				return true;
			}
		}
		return false;
	}

	internal void SetRect(Rectangle rect, Rectangle rect_text, int gap_x, int gap_x2, int gap_y, int gap_y2)
	{
		Rect = rect;
		if (Val is SelectItem)
		{
			if (Online > -1 || HasIcon)
			{
				RectCheck = new Rectangle(rect.X + gap_x / 2, rect_text.Y, rect_text.Height, rect_text.Height);
				int num = rect.X + rect_text.Height + gap_x;
				if (Online > -1 && HasIcon)
				{
					RectOnline = new Rectangle(num + (rect_text.Height - gap_y) / 2, rect_text.Y + (rect_text.Height - gap_y) / 2, gap_y, gap_y);
					RectIcon = new Rectangle(num + rect_text.Height, rect_text.Y, rect_text.Height, rect_text.Height);
					RectText = new Rectangle(num + gap_x / 2 + rect_text.Height * 2, rect_text.Y, rect_text.Width - num - rect_text.Height, rect_text.Height);
				}
				else if (Online > -1)
				{
					RectOnline = new Rectangle(num + (rect_text.Height - gap_y) / 2, rect_text.Y + (rect_text.Height - gap_y) / 2, gap_y, gap_y);
					RectText = new Rectangle(num + gap_x / 2 + rect_text.Height, rect_text.Y, rect_text.Width - num, rect_text.Height);
				}
				else
				{
					RectIcon = new Rectangle(num, rect_text.Y, rect_text.Height, rect_text.Height);
					RectText = new Rectangle(num + gap_x / 2 + rect_text.Height, rect_text.Y, rect_text.Width - num, rect_text.Height);
				}
			}
			else
			{
				RectCheck = new Rectangle(rect.X + gap_x / 2, rect_text.Y, rect_text.Height, rect_text.Height);
				RectText = new Rectangle(rect_text.X + rect_text.Height, rect_text.Y, rect_text.Width - rect_text.Height, rect_text.Height);
			}
			RectArrow = new Rectangle(Rect.Right - Rect.Height - gap_y, Rect.Y, Rect.Height, Rect.Height);
		}
		else
		{
			RectCheck = new Rectangle(rect.X + gap_x / 2, rect_text.Y, rect_text.Height, rect_text.Height);
			RectText = new Rectangle(rect_text.X + rect_text.Height, rect_text.Y, rect_text.Width - rect_text.Height, rect_text.Height);
		}
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
