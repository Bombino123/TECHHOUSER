using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class TreeItem
{
	private Image? icon;

	private string? iconSvg;

	private string? text;

	private string? subTitle;

	private bool visible = true;

	internal TreeItemCollection? items;

	private bool enabled = true;

	private ITask? ThreadExpand;

	private bool expand;

	internal bool AnimationCheck;

	internal float AnimationCheckValue;

	private ITask? ThreadCheck;

	private bool _checked;

	internal CheckState checkStateOld;

	private CheckState checkState;

	private Color? fore;

	private Color? back;

	private bool hover;

	private bool select;

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

	[Description("子标题")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? SubTitle
	{
		get
		{
			return Localization.GetLangI(LocalizationSubTitle, subTitle, new string[2] { "{id}", ID });
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = null;
			}
			if (!(subTitle == value))
			{
				subTitle = value;
				Invalidates();
			}
		}
	}

	[Description("子标题")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSubTitle { get; set; }

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
	public TreeItemCollection Sub
	{
		get
		{
			if (items == null)
			{
				items = new TreeItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

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

	[Description("展开")]
	[Category("行为")]
	[DefaultValue(false)]
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
				if (PARENT != null && ((Control)PARENT).IsHandleCreated && Config.Animation)
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
						if (num2 < 100)
						{
							num2 = 100;
						}
						else if (num2 > 1000)
						{
							num2 = 1000;
						}
						int totalFrames = Animation.TotalFrames(10, num2);
						ThreadExpand = new ITask(_is: false, 10, totalFrames, cold, AnimationType.Ball, delegate(int i, float val)
						{
							ExpandProg = val;
							Invalidates();
						}, delegate
						{
							ExpandProg = 1f;
							ExpandThread = false;
							Invalidates();
						});
					}
					else
					{
						int totalFrames2 = Animation.TotalFrames(10, 200);
						ThreadExpand = new ITask(_is: true, 10, totalFrames2, cold, AnimationType.Ball, delegate(int i, float val)
						{
							ExpandProg = val;
							Invalidates();
						}, delegate
						{
							ExpandProg = 1f;
							ExpandThread = false;
							Invalidates();
						});
					}
				}
				else
				{
					ExpandProg = 1f;
					Invalidates();
				}
			}
			else
			{
				ExpandProg = 1f;
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

	[Description("选中状态")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool Checked
	{
		get
		{
			return _checked;
		}
		set
		{
			if (_checked != value)
			{
				_checked = value;
				PARENT?.OnCheckedChanged(this, value);
				OnCheck();
				CheckState = (CheckState)(value ? 1 : 0);
			}
		}
	}

	[Description("选中状态")]
	[Category("行为")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public CheckState CheckState
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return checkState;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (checkState != value)
			{
				checkState = value;
				bool flag = (int)value == 1;
				if (_checked != flag)
				{
					_checked = flag;
					PARENT?.OnCheckedChanged(this, flag);
					OnCheck();
				}
				if ((int)value != 0)
				{
					checkStateOld = value;
				}
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

	internal float SubY { get; set; }

	internal float SubHeight { get; set; }

	internal float ExpandHeight { get; set; }

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
			PARENT?.OnNodeMouseMove(this, value);
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
					PARENT?.USelect();
				}
				select = value;
				Invalidate();
			}
		}
	}

	public int Depth { get; private set; }

	internal Tree? PARENT { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public TreeItem? PARENTITEM { get; set; }

	internal Rectangle rect { get; set; }

	internal Rectangle arr_rect { get; set; }

	internal Rectangle check_rect { get; set; }

	internal Rectangle txt_rect { get; set; }

	internal Rectangle subtxt_rect { get; set; }

	internal Rectangle ico_rect { get; set; }

	public TreeItem()
	{
	}

	public TreeItem(string text)
	{
		Text = text;
	}

	public TreeItem(string text, Image? icon)
	{
		Text = text;
		Icon = icon;
	}

	internal int ExpandCount(TreeItem it)
	{
		int num = 0;
		if (it.items != null && it.items.Count > 0)
		{
			num += it.items.Count;
			foreach (TreeItem item in it.items)
			{
				if (item.Expand)
				{
					num += ExpandCount(item);
				}
			}
		}
		return num;
	}

	private void OnCheck()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Invalid comparison between Unknown and I4
		ThreadCheck?.Dispose();
		ThreadCheck = null;
		if (PARENT != null && ((Control)PARENT).IsHandleCreated && (PARENTITEM == null || PARENTITEM.expand) && show && Config.Animation)
		{
			AnimationCheck = true;
			if (_checked)
			{
				ThreadCheck = new ITask((Control)(object)PARENT, delegate
				{
					AnimationCheckValue = AnimationCheckValue.Calculate(0.2f);
					if (AnimationCheckValue > 1f)
					{
						AnimationCheckValue = 1f;
						return false;
					}
					Invalidate();
					return true;
				}, 20, delegate
				{
					AnimationCheck = false;
					Invalidate();
				});
				return;
			}
			if ((int)checkStateOld == 1 && (int)CheckState == 2)
			{
				AnimationCheck = false;
				AnimationCheckValue = 1f;
				Invalidate();
				return;
			}
			ThreadCheck = new ITask((Control)(object)PARENT, delegate
			{
				AnimationCheckValue = AnimationCheckValue.Calculate(-0.2f);
				if (AnimationCheckValue <= 0f)
				{
					AnimationCheckValue = 0f;
					return false;
				}
				Invalidate();
				return true;
			}, 20, delegate
			{
				AnimationCheck = false;
				Invalidate();
			});
		}
		else
		{
			AnimationCheckValue = (_checked ? 1f : 0f);
			Invalidate();
		}
	}

	private void Invalidate()
	{
		Tree? pARENT = PARENT;
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

	internal void SetRect(Canvas g, Font font, int depth, bool checkable, bool blockNode, bool has_sub, Rectangle _rect, int icon_size, int gap)
	{
		Depth = depth;
		rect = _rect;
		int num = _rect.X + gap + icon_size * depth;
		int y = _rect.Y + (_rect.Height - icon_size) / 2;
		if (has_sub)
		{
			arr_rect = new Rectangle(num, y, icon_size, icon_size);
			num += icon_size + gap;
		}
		if (checkable)
		{
			check_rect = new Rectangle(num, y, icon_size, icon_size);
			num += icon_size + gap;
		}
		if (HasIcon)
		{
			ico_rect = new Rectangle(num, y, icon_size, icon_size);
			num += icon_size + gap;
		}
		Size size = g.MeasureString(Text, font);
		txt_rect = new Rectangle(num, _rect.Y + (_rect.Height - size.Height) / 2, size.Width, size.Height);
		if (SubTitle != null)
		{
			Size size2 = g.MeasureString(SubTitle, font);
			subtxt_rect = new Rectangle(txt_rect.Right, txt_rect.Y, size2.Width, txt_rect.Height);
			if (!blockNode)
			{
				rect = new Rectangle(txt_rect.X, txt_rect.Y, txt_rect.Width + subtxt_rect.Width, subtxt_rect.Height);
			}
		}
		else if (!blockNode)
		{
			rect = txt_rect;
		}
		Show = true;
	}

	internal int Contains(int x, int y, int sx, int sy, bool checkable, bool blockNode)
	{
		if (visible && enabled)
		{
			if (blockNode)
			{
				sx = 0;
				if (rect.Contains(x + sx, y + sy))
				{
					Hover = true;
					return 1;
				}
				if (arr_rect.Contains(x + sx, y + sy) && CanExpand)
				{
					Hover = rect.Contains(arr_rect);
					return 2;
				}
				if (checkable && check_rect.Contains(x + sx, y + sy))
				{
					Hover = rect.Contains(arr_rect);
					return 3;
				}
			}
			else
			{
				if (rect.Contains(x + sx, y + sy) || ico_rect.Contains(x + sx, y + sy))
				{
					Hover = true;
					return 1;
				}
				if (arr_rect.Contains(x + sx, y + sy) && CanExpand)
				{
					Hover = rect.Contains(arr_rect);
					return 2;
				}
				if (checkable && check_rect.Contains(x + sx, y + sy))
				{
					Hover = rect.Contains(arr_rect);
					return 3;
				}
			}
		}
		Hover = false;
		return 0;
	}

	public Rectangle Rect(string type = "", bool actual = true)
	{
		if (actual || PARENT == null)
		{
			return Rect(type);
		}
		return Rect(type, PARENT.ScrollBar.ValueX, PARENT.ScrollBar.ValueY);
	}

	public Rectangle Rect(string type = "", int sx = 0, int sy = 0)
	{
		if (sx > 0 || sy > 0)
		{
			return type switch
			{
				"Text" => new Rectangle(txt_rect.X - sx, txt_rect.Y - sy, txt_rect.Width, txt_rect.Height), 
				"SubTitle" => new Rectangle(subtxt_rect.X - sx, subtxt_rect.Y - sy, subtxt_rect.Width, subtxt_rect.Height), 
				"Checked" => new Rectangle(check_rect.X - sx, check_rect.Y - sy, check_rect.Width, check_rect.Height), 
				"Icon" => new Rectangle(ico_rect.X - sx, ico_rect.Y - sy, ico_rect.Width, ico_rect.Height), 
				"Arrow" => new Rectangle(arr_rect.X - sx, arr_rect.Y - sy, arr_rect.Width, arr_rect.Height), 
				_ => new Rectangle(rect.X - sx, rect.Y - sy, rect.Width, rect.Height), 
			};
		}
		return type switch
		{
			"Text" => txt_rect, 
			"SubTitle" => subtxt_rect, 
			"Checked" => check_rect, 
			"Icon" => ico_rect, 
			"Arrow" => arr_rect, 
			_ => rect, 
		};
	}

	public override string? ToString()
	{
		return Text;
	}
}
