using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class CollapseGroupItem
{
	private string? text;

	internal CollapseGroupSubCollection? items;

	private ITask? ThreadExpand;

	private bool expand;

	private Color? fore;

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

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("子集合")]
	[Category("外观")]
	public CollapseGroupSubCollection Sub
	{
		get
		{
			if (items == null)
			{
				items = new CollapseGroupSubCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
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
						int totalFrames = Animation.TotalFrames(10, 200);
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
			if (items != null)
			{
				return items.Count > 0;
			}
			return false;
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

	internal float SubY { get; set; }

	internal float SubHeight { get; set; }

	internal float ExpandHeight { get; set; }

	internal float ExpandProg { get; set; }

	internal bool ExpandThread { get; set; }

	internal bool Show { get; set; }

	internal CollapseGroup? PARENT { get; set; }

	internal Rectangle rect { get; set; }

	internal Rectangle arr_rect { get; set; }

	internal Rectangle txt_rect { get; set; }

	public CollapseGroupItem()
	{
	}

	public CollapseGroupItem(string text)
	{
		Text = text;
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

	internal void SetRect(Canvas g, Rectangle _rect, int icon_size, int gap)
	{
		rect = _rect;
		int num = _rect.X + gap;
		int y = _rect.Y + (_rect.Height - icon_size) / 2;
		arr_rect = new Rectangle(num, y, icon_size, icon_size);
		num += icon_size + gap;
		txt_rect = new Rectangle(num, _rect.Y, _rect.Width - num - gap, _rect.Height);
		Show = true;
	}

	public override string? ToString()
	{
		return text;
	}
}
