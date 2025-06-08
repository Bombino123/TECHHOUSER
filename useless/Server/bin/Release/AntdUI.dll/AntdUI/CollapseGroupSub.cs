using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class CollapseGroupSub
{
	private Image? icon;

	private string? iconSvg;

	private string? text;

	private bool enabled = true;

	private Color? fore;

	private Color? back;

	private bool hover;

	private bool select;

	internal float AnimationHoverValue;

	internal bool AnimationHover;

	private ITask? ThreadHover;

	[Description("图标")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Icon
	{
		get
		{
			return icon;
		}
		set
		{
			if (icon != value)
			{
				icon = value;
				Invalidates();
			}
		}
	}

	[Description("图标SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? IconSvg
	{
		get
		{
			return iconSvg;
		}
		set
		{
			if (!(iconSvg == value))
			{
				iconSvg = value;
				Invalidates();
			}
		}
	}

	internal bool HasIcon
	{
		get
		{
			if (iconSvg == null)
			{
				return Icon != null;
			}
			return true;
		}
	}

	[Description("名称")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? Name { get; set; }

	[Description("文本")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? Text
	{
		get
		{
			return text;
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				Invalidates();
			}
		}
	}

	[Description("用户定义数据")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	[Description("禁掉响应")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled != value)
			{
				enabled = value;
				Invalidate();
			}
		}
	}

	[Description("文本颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	public Color? Fore
	{
		get
		{
			return fore;
		}
		set
		{
			if (!(fore == value))
			{
				fore = value;
				Invalidate();
			}
		}
	}

	[Description("背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	public Color? Back
	{
		get
		{
			return back;
		}
		set
		{
			if (!(back == value))
			{
				back = value;
				Invalidate();
			}
		}
	}

	internal bool Hover
	{
		get
		{
			return hover;
		}
		set
		{
			if (hover == value)
			{
				return;
			}
			hover = value;
			if (Config.Animation)
			{
				ThreadHover?.Dispose();
				AnimationHover = true;
				int t = Animation.TotalFrames(20, 200);
				if (value)
				{
					ThreadHover = new ITask(delegate(int i)
					{
						AnimationHoverValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
						Invalidate();
						return true;
					}, 20, t, delegate
					{
						AnimationHover = false;
						AnimationHoverValue = 1f;
						Invalidate();
					});
				}
				else
				{
					ThreadHover = new ITask(delegate(int i)
					{
						AnimationHoverValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
						Invalidate();
						return true;
					}, 20, t, delegate
					{
						AnimationHover = false;
						AnimationHoverValue = 0f;
						Invalidate();
					});
				}
			}
			else
			{
				Invalidate();
			}
		}
	}

	[Description("激活状态")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool Select
	{
		get
		{
			return select;
		}
		set
		{
			if (select != value)
			{
				if (value)
				{
					PARENT?.IUSelect();
				}
				select = value;
				Invalidate();
			}
		}
	}

	internal CollapseGroup? PARENT { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public CollapseGroupItem? PARENTITEM { get; set; }

	internal bool Show { get; set; }

	internal Rectangle rect { get; set; }

	internal Rectangle txt_rect { get; set; }

	internal Rectangle ico_rect { get; set; }

	public CollapseGroupSub()
	{
	}

	public CollapseGroupSub(string text)
	{
		Text = text;
	}

	public CollapseGroupSub(string text, Image? icon)
	{
		Text = text;
		Icon = icon;
	}

	private void Invalidate()
	{
		CollapseGroup? pARENT = PARENT;
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

	internal void SetRect(Canvas g, Rectangle rect_read, int font_height, int xc, int icon_size)
	{
		rect = rect_read;
		int num = (int)((float)font_height * 0.25f);
		int num2 = rect_read.Y + (rect_read.Height - (font_height + icon_size + num)) / 2;
		txt_rect = new Rectangle(rect_read.X, num2 + icon_size + num, rect_read.Width, rect_read.Height);
		ico_rect = new Rectangle(rect_read.X + (rect_read.Width - icon_size) / 2, num2, icon_size, icon_size);
		if (xc > 0)
		{
			rect = new Rectangle(rect_read.X, rect_read.Y, rect_read.Width, rect_read.Height + xc);
		}
		Show = true;
	}

	internal bool Contains(Point point, int x, int y)
	{
		if (rect.Contains(point.X + x, point.Y + y))
		{
			Hover = true;
			return true;
		}
		Hover = false;
		return false;
	}

	public override string? ToString()
	{
		return text;
	}
}
