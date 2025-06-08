using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI.Chat;

public class MsgItem
{
	private Image? _icon;

	private string _name;

	private string? _text;

	private int count;

	private string? time;

	private bool visible = true;

	internal bool Hover;

	internal bool select;

	[Description("ID")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? ID { get; set; }

	[Description("图标")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Icon
	{
		get
		{
			return _icon;
		}
		set
		{
			if (_icon != value)
			{
				_icon = value;
				Invalidates();
			}
		}
	}

	[Description("名称")]
	[Category("外观")]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (!(_name == value))
			{
				_name = value;
				Invalidate();
			}
		}
	}

	[Description("文本")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (!(_text == value))
			{
				_text = value;
				Invalidate();
			}
		}
	}

	[Description("消息数量")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Count
	{
		get
		{
			return count;
		}
		set
		{
			if (count != value)
			{
				count = value;
				Invalidates();
			}
		}
	}

	[Description("时间")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? Time
	{
		get
		{
			return time;
		}
		set
		{
			if (!(time == value))
			{
				time = value;
				Invalidates();
			}
		}
	}

	[Description("是否显示")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			if (visible != value)
			{
				visible = value;
				Invalidates();
			}
		}
	}

	[Description("用户定义数据")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	internal bool show { get; set; }

	internal bool Show { get; set; }

	[Description("是否选中")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Select
	{
		get
		{
			return select;
		}
		set
		{
			if (select == value)
			{
				return;
			}
			select = value;
			if (value && PARENT != null)
			{
				foreach (MsgItem item in PARENT.Items)
				{
					if (item != this)
					{
						item.select = false;
					}
				}
			}
			Invalidate();
		}
	}

	internal MsgList? PARENT { get; set; }

	internal Rectangle rect { get; set; }

	internal Rectangle rect_name { get; set; }

	internal Rectangle rect_time { get; set; }

	internal Rectangle rect_text { get; set; }

	internal Rectangle rect_icon { get; set; }

	public MsgItem()
	{
	}

	public MsgItem(string name)
	{
		_name = name;
	}

	public MsgItem(string name, Bitmap? icon)
	{
		_name = name;
		_icon = (Image?)(object)icon;
	}

	private void Invalidate()
	{
		MsgList? pARENT = PARENT;
		if (pARENT != null)
		{
			((Control)pARENT).Invalidate();
		}
	}

	private void Invalidates()
	{
		if (PARENT != null)
		{
			PARENT.ChangeList();
			((Control)PARENT).Invalidate();
		}
	}

	internal void SetRect(Rectangle _rect, int time_width, int gap, int spilt, int gap_name, int gap_desc, int image_size, int name_height, int desc_height)
	{
		rect = _rect;
		int num = _rect.Width - image_size - gap - spilt * 2;
		rect_icon = new Rectangle(_rect.X + gap, _rect.Y + gap, image_size, image_size);
		rect_name = new Rectangle(rect_icon.Right + spilt, rect_icon.Y + gap_name - gap_desc, num - time_width, name_height + gap_desc * 2);
		rect_time = new Rectangle(rect_name.Right, rect_name.Y, time_width, rect_name.Height);
		rect_text = new Rectangle(rect_name.X, rect_icon.Bottom - gap_desc - desc_height - gap_desc, num, desc_height + gap_desc * 2);
		Show = true;
	}

	internal bool Contains(Point point, int x, int y, out bool change)
	{
		if (rect.Contains(new Point(point.X + x, point.Y + y)))
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
}
