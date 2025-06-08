using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class MenuItem
{
	private Image? icon;

	private string? iconSvg;

	private string? text;

	private bool visible = true;

	internal MenuItemCollection? items;

	private bool enabled = true;

	private ITask? ThreadExpand;

	private bool expand = true;

	private bool hover;

	internal float AnimationHoverValue;

	internal bool AnimationHover;

	private ITask? ThreadHover;

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
			if (string.IsNullOrWhiteSpace(iconSvg))
			{
				return icon != null;
			}
			return true;
		}
	}

	[Description("图标激活")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? IconActive { get; set; }

	[Description("图标激活SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? IconActiveSvg { get; set; }

	[Description("文本")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? Text
	{
		get
		{
			return Localization.GetLangI(LocalizationText, text, new string[2] { "{id}", ID });
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

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("自定义字体")]
	[Category("外观")]
	[DefaultValue(null)]
	public Font? Font { get; set; }

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

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("子集合")]
	[Category("外观")]
	public MenuItemCollection Sub
	{
		get
		{
			if (items == null)
			{
				items = new MenuItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	[Description("禁用状态")]
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

	[Description("展开")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool Expand
	{
		get
		{
			return expand;
		}
		set
		{
			if (expand == value)
			{
				return;
			}
			expand = value;
			if (items != null && items.Count > 0)
			{
				if (value && PARENT != null && PARENT.Unique)
				{
					if (PARENTITEM == null)
					{
						foreach (MenuItem item in PARENT.Items)
						{
							if (item != this)
							{
								item.Expand = false;
							}
						}
					}
					else
					{
						foreach (MenuItem item2 in PARENTITEM.Sub)
						{
							if (item2 != this)
							{
								item2.Expand = false;
							}
						}
					}
				}
				if (Config.Animation)
				{
					ThreadExpand?.Dispose();
					float cold = -1f;
					if (ThreadExpand?.Tag is float num)
					{
						cold = num;
					}
					ExpandThread = true;
					if (value)
					{
						int num2 = ExpandCount(this) * 10;
						if (num2 > 1000)
						{
							num2 = 1000;
						}
						int t2 = Animation.TotalFrames(10, num2);
						ThreadExpand = new ITask(_is: false, 10, t2, cold, AnimationType.Ball, delegate(int i, float val)
						{
							ExpandProg = val;
							ArrowProg = Animation.Animate(i, t2, 2f, AnimationType.Ball) - 1f;
							Invalidates();
						}, delegate
						{
							ArrowProg = 1f;
							ExpandProg = 1f;
							ExpandThread = false;
							Invalidates();
						});
					}
					else
					{
						int t = Animation.TotalFrames(10, 200);
						ThreadExpand = new ITask(_is: true, 10, t, cold, AnimationType.Ball, delegate(int i, float val)
						{
							ExpandProg = val;
							ArrowProg = 0f - (Animation.Animate(i, t, 2f, AnimationType.Ball) - 1f);
							Invalidates();
						}, delegate
						{
							ExpandProg = 1f;
							ExpandThread = false;
							ArrowProg = -1f;
							Invalidates();
						});
					}
				}
				else
				{
					ExpandProg = 1f;
					ArrowProg = (value ? 1f : (-1f));
					Invalidates();
				}
			}
			else
			{
				expand = false;
				ExpandProg = 1f;
				ArrowProg = -1f;
				Invalidates();
			}
		}
	}

	[Description("是否可以展开")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool CanExpand
	{
		get
		{
			if (visible && items != null)
			{
				return items.Count > 0;
			}
			return false;
		}
	}

	public Rectangle Rect
	{
		get
		{
			if (PARENT == null)
			{
				return rect;
			}
			int value = PARENT.ScrollBar.Value;
			if ((float)value != 0f)
			{
				return new Rectangle(rect.X, rect.Y - value, rect.Width, rect.Height);
			}
			return rect;
		}
	}

	internal float SubY { get; set; }

	internal float SubHeight { get; set; }

	internal int ExpandHeight { get; set; }

	internal float ExpandProg { get; set; }

	internal bool ExpandThread { get; set; }

	internal bool show { get; set; }

	internal bool Show { get; set; }

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
			if (!Config.Animation)
			{
				return;
			}
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
	}

	[Description("是否选中")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Select { get; set; }

	internal int Depth { get; set; }

	internal float ArrowProg { get; set; } = 1f;


	internal Menu? PARENT { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public MenuItem? PARENTITEM { get; set; }

	internal Rectangle rect { get; set; }

	internal Rectangle arr_rect { get; set; }

	internal Rectangle txt_rect { get; set; }

	internal Rectangle ico_rect { get; set; }

	public MenuItem()
	{
	}

	public MenuItem(string text)
	{
		Text = text;
	}

	public MenuItem(string text, Bitmap? icon)
	{
		Text = text;
		Icon = (Image?)(object)icon;
	}

	public MenuItem(string text, string? icon_svg)
	{
		Text = text;
		IconSvg = icon_svg;
	}

	internal int ExpandCount(MenuItem it)
	{
		int num = 0;
		if (it.Sub != null && it.Sub.Count > 0)
		{
			num += it.Sub.Count;
			foreach (MenuItem item in it.Sub)
			{
				if (item.Expand)
				{
					num += ExpandCount(item);
				}
			}
		}
		return num;
	}

	private void Invalidate()
	{
		Menu? pARENT = PARENT;
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

	internal void SetRect(int depth, bool indent, Rectangle _rect, int icon_size, int gap)
	{
		Depth = depth;
		rect = _rect;
		if (HasIcon)
		{
			if (indent || depth > 1)
			{
				ico_rect = new Rectangle(_rect.X + gap * (depth + 1), _rect.Y + (_rect.Height - icon_size) / 2, icon_size, icon_size);
				txt_rect = new Rectangle(ico_rect.X + ico_rect.Width + gap, _rect.Y, _rect.Width - (ico_rect.Width + gap * 2), _rect.Height);
			}
			else
			{
				ico_rect = new Rectangle(_rect.X + gap, _rect.Y + (_rect.Height - icon_size) / 2, icon_size, icon_size);
				txt_rect = new Rectangle(ico_rect.X + ico_rect.Width + gap, _rect.Y, _rect.Width - (ico_rect.Width + gap * 2), _rect.Height);
			}
			arr_rect = new Rectangle(_rect.Right - ico_rect.Height - (int)((float)ico_rect.Height * 0.9f), _rect.Y + (_rect.Height - ico_rect.Height) / 2, ico_rect.Height, ico_rect.Height);
		}
		else
		{
			if (indent || depth > 1)
			{
				txt_rect = new Rectangle(_rect.X + gap * (depth + 1), _rect.Y, _rect.Width - gap * 2, _rect.Height);
			}
			else
			{
				txt_rect = new Rectangle(_rect.X + gap, _rect.Y, _rect.Width - gap * 2, _rect.Height);
			}
			arr_rect = new Rectangle(_rect.Right - icon_size - (int)((float)icon_size * 0.9f), _rect.Y + (_rect.Height - icon_size) / 2, icon_size, icon_size);
		}
		Show = true;
	}

	internal void SetRectNoArr(int depth, Rectangle _rect, int icon_size, int gap)
	{
		Depth = depth;
		rect = _rect;
		if (HasIcon)
		{
			ico_rect = new Rectangle(_rect.X + gap, _rect.Y + (_rect.Height - icon_size) / 2, icon_size, icon_size);
			txt_rect = new Rectangle(ico_rect.X + ico_rect.Width + gap, _rect.Y, _rect.Width - (ico_rect.Width + gap * 2), _rect.Height);
		}
		else
		{
			txt_rect = new Rectangle(_rect.X + gap, _rect.Y, _rect.Width - gap * 2, _rect.Height);
		}
		Show = true;
	}

	internal bool Contains(Point point, int x, int y, out bool change)
	{
		if (rect.Contains(point.X + x, point.Y + y))
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
			Hover = true;
		}
		else
		{
			if (hover)
			{
				result = true;
			}
			Hover = false;
		}
		return result;
	}

	public override string? ToString()
	{
		return Text;
	}
}
