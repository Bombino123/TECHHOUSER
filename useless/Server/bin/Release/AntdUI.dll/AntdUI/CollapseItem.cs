using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

[ToolboxItem(false)]
[Designer(typeof(IControlDesigner))]
public class CollapseItem : ScrollableControl
{
	private ITask? ThreadExpand;

	private bool expand;

	private bool full;

	private string text = "";

	internal bool MDown;

	internal Rectangle Rect = new Rectangle(-10, -10, 0, 0);

	internal Rectangle RectArrow;

	internal Rectangle RectCcntrol;

	internal Rectangle RectTitle;

	internal Rectangle RectText;

	internal Collapse? PARENT;

	private bool canset = true;

	protected override Size DefaultSize => new Size(100, 60);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Category("外观")]
	[Description("展开进度")]
	[DefaultValue(0f)]
	internal float ExpandProg { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Category("外观")]
	[Description("展开状态")]
	[DefaultValue(false)]
	internal bool ExpandThread { get; set; }

	[Category("外观")]
	[Description("是否展开")]
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
			PARENT?.OnExpandChanged(this, expand);
			if (value)
			{
				PARENT?.UniqueOne(this);
			}
			if (PARENT != null && ((Control)PARENT).IsHandleCreated && Config.Animation)
			{
				((Control)this).Location = new Point(-((Control)this).Width, -((Control)this).Height);
				ThreadExpand?.Dispose();
				float cold = -1f;
				if (ThreadExpand?.Tag is float num)
				{
					cold = num;
				}
				ExpandThread = true;
				int totalFrames = Animation.TotalFrames(10, 200);
				if (value)
				{
					ThreadExpand = new ITask(_is: false, 10, totalFrames, cold, AnimationType.Ball, delegate(int i, float val)
					{
						ExpandProg = val;
						PARENT.LoadLayout();
					}, delegate
					{
						ExpandProg = 1f;
						ExpandThread = false;
						PARENT.LoadLayout();
					});
				}
				else
				{
					ThreadExpand = new ITask(_is: true, 10, totalFrames, cold, AnimationType.Ball, delegate(int i, float val)
					{
						ExpandProg = val;
						PARENT.LoadLayout();
					}, delegate
					{
						ExpandProg = 1f;
						ExpandThread = false;
						PARENT.LoadLayout();
					});
				}
			}
			else
			{
				PARENT?.LoadLayout();
				if (!value)
				{
					((Control)this).Location = new Point(-((Control)this).Width, -((Control)this).Height);
				}
			}
		}
	}

	[Category("外观")]
	[Description("是否铺满剩下空间")]
	[DefaultValue(false)]
	public bool Full
	{
		get
		{
			return full;
		}
		set
		{
			if (full != value)
			{
				full = value;
				PARENT?.LoadLayout();
			}
		}
	}

	[Description("文本")]
	[Category("外观")]
	[DefaultValue("")]
	public override string Text
	{
		get
		{
			return ((Control)(object)this).GetLangIN(LocalizationText, text);
		}
		set
		{
			if (!(text == value))
			{
				((Control)this).Text = (text = value);
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	public CollapseItem()
	{
		((Control)this).SetStyle((ControlStyles)206867, true);
		((Control)this).UpdateStyles();
	}

	internal bool Contains(int x, int y)
	{
		return RectTitle.Contains(x, y);
	}

	protected override void OnTextChanged(EventArgs e)
	{
		PARENT?.LoadLayout();
		((Control)this).OnTextChanged(e);
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		PARENT?.LoadLayout();
		((ScrollableControl)this).OnVisibleChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		if (canset)
		{
			PARENT?.LoadLayout();
		}
		((Control)this).OnSizeChanged(e);
	}

	public void SetSize()
	{
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)new Action(SetSize));
			return;
		}
		canset = false;
		((Control)this).Size = RectCcntrol.Size;
		((Control)this).Location = RectCcntrol.Location;
		canset = true;
	}

	public override string ToString()
	{
		return ((Control)this).Text;
	}
}
