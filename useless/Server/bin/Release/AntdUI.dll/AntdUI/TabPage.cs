using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

[ToolboxItem(false)]
[Designer(typeof(IControlDesigner))]
public class TabPage : ScrollableControl
{
	private DockStyle dock = (DockStyle)5;

	private Image? icon;

	private string? iconSvg;

	private bool readOnly;

	private string? badge;

	private float badgeSize = 0.6f;

	private Color? badgeback;

	private string text = "";

	internal bool MDown;

	internal Rectangle Rect = new Rectangle(-10, -10, 0, 0);

	internal Tabs? PARENT;

	[Category("布局")]
	[Description("定义要绑定到容器的控件边框")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public DockStyle Dock
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return dock;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			dock = value;
		}
	}

	[Category("外观")]
	[Description("图标")]
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
				PARENT?.LoadLayout();
			}
		}
	}

	[Category("外观")]
	[Description("图标SVG")]
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
				PARENT?.LoadLayout();
			}
		}
	}

	public bool HasIcon
	{
		get
		{
			if (iconSvg == null)
			{
				return icon != null;
			}
			return true;
		}
	}

	[Description("只读")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ReadOnly
	{
		get
		{
			return readOnly;
		}
		set
		{
			if (readOnly != value)
			{
				readOnly = value;
				PARENT?.LoadLayout();
			}
		}
	}

	[Description("徽标内容")]
	[Category("徽标")]
	[DefaultValue(null)]
	public string? Badge
	{
		get
		{
			return badge;
		}
		set
		{
			if (!(badge == value))
			{
				badge = value;
				Tabs? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	[Description("徽标比例")]
	[Category("徽标")]
	[DefaultValue(0.6f)]
	public float BadgeSize
	{
		get
		{
			return badgeSize;
		}
		set
		{
			if (badgeSize == value)
			{
				return;
			}
			badgeSize = value;
			if (badge != null)
			{
				Tabs? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	[Description("徽标背景颜色")]
	[Category("徽标")]
	[DefaultValue(null)]
	public Color? BadgeBack
	{
		get
		{
			return badgeback;
		}
		set
		{
			if (badgeback == value || !(badgeback != value))
			{
				return;
			}
			badgeback = value;
			if (badge != null)
			{
				Tabs? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	[Description("徽标偏移X")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetX { get; set; } = 1;


	[Description("徽标偏移Y")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetY { get; set; } = 1;


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

	public TabPage()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).SetStyle((ControlStyles)206867, true);
		((Control)this).UpdateStyles();
	}

	internal bool Contains(int x, int y)
	{
		return Rect.Contains(x, y);
	}

	internal Rectangle SetRect(Rectangle rect)
	{
		Rect = rect;
		return Rect;
	}

	internal Rectangle SetOffset(int x, int y)
	{
		Rect.Offset(x, y);
		return Rect;
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

	public void SetDock(bool isdock)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				SetDock(isdock);
			});
		}
		else if (isdock)
		{
			((Control)this).Dock = dock;
		}
		else
		{
			((Control)this).Dock = (DockStyle)0;
			((Control)this).Location = new Point(-((Control)this).Width, -((Control)this).Height);
		}
	}

	public override string ToString()
	{
		return ((Control)this).Text;
	}
}
