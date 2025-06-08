using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class StepsItem
{
	private Image? icon;

	private string? iconSvg;

	private int? iconsize;

	private string title = "Title";

	private string? subTitle;

	internal bool showSub;

	private string? description;

	internal bool showDescription;

	private bool visible = true;

	[Description("ID")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? ID { get; set; }

	[Description("图标，可选")]
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
				Steps? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	[Description("图标SVG，可选")]
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
				Steps? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	[Description("图标的大小，可选")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? IconSize
	{
		get
		{
			return iconsize;
		}
		set
		{
			if (iconsize != value)
			{
				iconsize = value;
				Invalidate();
			}
		}
	}

	internal int ReadWidth { get; set; }

	[Description("名称")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? Name { get; set; }

	[Description("标题")]
	[Category("外观")]
	[DefaultValue("Title")]
	public string Title
	{
		get
		{
			return Localization.GetLangIN(LocalizationTitle, title, new string[2] { "{id}", ID });
		}
		set
		{
			if (!(title == value))
			{
				title = value;
				Invalidate();
			}
		}
	}

	[Description("标题")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTitle { get; set; }

	internal Size TitleSize { get; set; }

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
				showSub = subTitle != null;
				Invalidate();
			}
		}
	}

	[Description("子标题")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSubTitle { get; set; }

	internal Size SubTitleSize { get; set; }

	[Description("详情描述，可选")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? Description
	{
		get
		{
			return Localization.GetLangI(LocalizationDescription, description, new string[2] { "{id}", ID });
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = null;
			}
			if (!(description == value))
			{
				description = value;
				showDescription = description != null;
				Invalidate();
			}
		}
	}

	[Description("详情描述")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationDescription { get; set; }

	internal Size DescriptionSize { get; set; }

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
				Invalidate();
			}
		}
	}

	[Description("用户定义数据")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	internal Steps? PARENT { get; set; }

	internal float pen_w { get; set; } = 3f;


	internal Rectangle rect { get; set; }

	internal Rectangle title_rect { get; set; }

	internal Rectangle subtitle_rect { get; set; }

	internal Rectangle description_rect { get; set; }

	internal Rectangle ico_rect { get; set; }

	public StepsItem()
	{
	}

	public StepsItem(string title)
	{
		Title = title;
	}

	public StepsItem(string title, string subTitle)
	{
		Title = title;
		SubTitle = subTitle;
	}

	public StepsItem(string title, string subTitle, string description)
	{
		Title = title;
		SubTitle = subTitle;
		Description = description;
	}

	private void Invalidate()
	{
		if (PARENT != null)
		{
			PARENT.ChangeList();
			((Control)PARENT).Invalidate();
		}
	}

	public override string ToString()
	{
		return Title + " " + SubTitle;
	}
}
