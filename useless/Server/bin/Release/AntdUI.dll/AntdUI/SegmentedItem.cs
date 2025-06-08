using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace AntdUI;

public class SegmentedItem : BadgeConfig
{
	private bool enabled = true;

	private Image? icon;

	private string? iconsvg;

	private string? text;

	private bool multiLine;

	private string? badge;

	private string? badgeSvg;

	private TAlignFrom badgeAlign = TAlignFrom.TR;

	private bool badgeMode;

	[Description("ID")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? ID { get; set; }

	[Description("使能")]
	[Category("外观")]
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
				Segmented? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

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
			return iconsvg;
		}
		set
		{
			if (!(iconsvg == value))
			{
				iconsvg = value;
				Invalidates();
			}
		}
	}

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

	[Description("图标激活")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? IconActive { get; set; }

	[Description("图标激活SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? IconActiveSvg { get; set; }

	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
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
				if (value == null)
				{
					multiLine = false;
				}
				else
				{
					multiLine = value.Contains("\n");
				}
				text = value;
				Invalidates();
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("用户定义数据")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	internal bool Hover { get; set; }

	internal bool HasEmptyText
	{
		get
		{
			if (Text != null)
			{
				return string.IsNullOrEmpty(Text);
			}
			return true;
		}
	}

	internal Rectangle Rect { get; set; }

	internal Rectangle RectImg { get; set; }

	internal Rectangle RectText { get; set; }

	internal Segmented? PARENT { get; set; }

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
				Segmented? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	public string? BadgeSvg
	{
		get
		{
			return badgeSvg;
		}
		set
		{
			if (!(badgeSvg == value))
			{
				badgeSvg = value;
				Segmented? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	public TAlignFrom BadgeAlign
	{
		get
		{
			return badgeAlign;
		}
		set
		{
			if (badgeAlign != value)
			{
				badgeAlign = value;
				Segmented? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	public float BadgeSize { get; set; } = 0.6f;


	public Color? BadgeBack { get; set; }

	public bool BadgeMode
	{
		get
		{
			return badgeMode;
		}
		set
		{
			if (badgeMode != value)
			{
				badgeMode = value;
				Segmented? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	public int BadgeOffsetX { get; set; }

	public int BadgeOffsetY { get; set; }

	internal void SetOffset(int x, int y)
	{
		Rect = new Rectangle(Rect.X + x, Rect.Y + y, Rect.Width, Rect.Height);
		RectImg = new Rectangle(RectImg.X + x, RectImg.Y + y, RectImg.Width, RectImg.Height);
		RectText = new Rectangle(RectText.X + x, RectText.Y + y, RectText.Width, RectText.Height);
	}

	internal void SetIconNoText(Rectangle rect, int imgsize)
	{
		Rect = rect;
		Rectangle rectImg = (RectText = new Rectangle(rect.X + (rect.Width - imgsize) / 2, rect.Y + (rect.Height - imgsize) / 2, imgsize, imgsize));
		RectImg = rectImg;
	}

	internal void SetRectTop(Rectangle rect, int imgsize, int text_heigth, int gap)
	{
		Rect = rect;
		if (HasIcon)
		{
			int num = (rect.Height - (imgsize + text_heigth + gap)) / 2;
			RectImg = new Rectangle(rect.X + (rect.Width - imgsize) / 2, rect.Y + num, imgsize, imgsize);
			RectText = new Rectangle(rect.X, RectImg.Bottom + gap, rect.Width, text_heigth);
		}
		else
		{
			RectText = rect;
		}
	}

	internal void SetRectTop(Rectangle rect, int imgsize, int text_heigth, int gap, Canvas g, Font font)
	{
		Rect = rect;
		if (HasIcon)
		{
			if (multiLine)
			{
				int height = g.MeasureString(Text, font).Height;
				if (height > text_heigth)
				{
					rect.Height += height - text_heigth;
					Rect = rect;
					text_heigth = height;
				}
			}
			int num = (rect.Height - (imgsize + text_heigth + gap)) / 2;
			RectImg = new Rectangle(rect.X + (rect.Width - imgsize) / 2, rect.Y + num, imgsize, imgsize);
			RectText = new Rectangle(rect.X, RectImg.Bottom + gap, rect.Width, text_heigth);
		}
		else
		{
			RectText = rect;
		}
	}

	internal void SetRectTopFull(Rectangle rect, int imgsize, int text_heigth, int gap, Canvas g, Font font)
	{
		Rect = rect;
		if (HasIcon)
		{
			if (multiLine)
			{
				int height = g.MeasureString(Text, font).Height;
				if (height > text_heigth)
				{
					text_heigth = height;
				}
			}
			int num = (rect.Height - (imgsize + text_heigth + gap)) / 2;
			RectImg = new Rectangle(rect.X + (rect.Width - imgsize) / 2, rect.Y + num, imgsize, imgsize);
			RectText = new Rectangle(rect.X, RectImg.Bottom + gap, rect.Width, text_heigth);
		}
		else
		{
			RectText = rect;
		}
	}

	internal void SetRectBottom(Rectangle rect, int imgsize, int text_heigth, int gap)
	{
		Rect = rect;
		if (HasIcon)
		{
			int num = (rect.Height - (imgsize + text_heigth + gap)) / 2;
			RectText = new Rectangle(rect.X, rect.Y + num, rect.Width, text_heigth);
			RectImg = new Rectangle(rect.X + (rect.Width - imgsize) / 2, RectText.Bottom + gap, imgsize, imgsize);
		}
		else
		{
			RectText = rect;
		}
	}

	internal void SetRectBottom(Rectangle rect, int imgsize, int text_heigth, int gap, Canvas g, Font font)
	{
		Rect = rect;
		if (HasIcon)
		{
			if (multiLine)
			{
				int height = g.MeasureString(Text, font).Height;
				if (height > text_heigth)
				{
					rect.Height += height - text_heigth;
					Rect = rect;
					text_heigth = height;
				}
			}
			int num = (rect.Height - (imgsize + text_heigth + gap)) / 2;
			RectText = new Rectangle(rect.X, rect.Y + num, rect.Width, text_heigth);
			RectImg = new Rectangle(rect.X + (rect.Width - imgsize) / 2, RectText.Bottom + gap, imgsize, imgsize);
		}
		else
		{
			RectText = rect;
		}
	}

	internal void SetRectBottomFull(Rectangle rect, int imgsize, int text_heigth, int gap, Canvas g, Font font)
	{
		Rect = rect;
		if (HasIcon)
		{
			if (multiLine)
			{
				int height = g.MeasureString(Text, font).Height;
				if (height > text_heigth)
				{
					text_heigth = height;
				}
			}
			int num = (rect.Height - (imgsize + text_heigth + gap)) / 2;
			RectText = new Rectangle(rect.X, rect.Y + num, rect.Width, text_heigth);
			RectImg = new Rectangle(rect.X + (rect.Width - imgsize) / 2, RectText.Bottom + gap, imgsize, imgsize);
		}
		else
		{
			RectText = rect;
		}
	}

	internal void SetRectLeft(Rectangle rect, int imgsize, int gap, int sp)
	{
		Rect = rect;
		if (HasIcon)
		{
			RectImg = new Rectangle(rect.X + sp, rect.Y + (rect.Height - imgsize) / 2, imgsize, imgsize);
			RectText = new Rectangle(RectImg.Right + gap, rect.Y, rect.Width - sp - imgsize - gap, rect.Height);
		}
		else
		{
			RectText = rect;
		}
	}

	internal void SetRectRight(Rectangle rect, int imgsize, int gap, int sp)
	{
		Rect = rect;
		if (HasIcon)
		{
			RectText = new Rectangle(rect.X, rect.Y, rect.Width - sp - imgsize - gap, rect.Height);
			RectImg = new Rectangle(RectText.Right + gap, rect.Y + (rect.Height - imgsize) / 2, imgsize, imgsize);
		}
		else
		{
			RectText = rect;
		}
	}

	internal void SetRectNone(Rectangle rect)
	{
		Rect = rect;
		RectText = rect;
	}

	private void Invalidates()
	{
		if (PARENT != null)
		{
			PARENT.ChangeItems();
			((Control)PARENT).Invalidate();
		}
	}

	public override string? ToString()
	{
		return Text;
	}
}
