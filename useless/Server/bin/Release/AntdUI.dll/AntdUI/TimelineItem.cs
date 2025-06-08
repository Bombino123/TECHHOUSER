using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

public class TimelineItem
{
	private string? description;

	private string? text;

	private bool visible = true;

	[Description("ID")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? ID { get; set; }

	[Description("图标")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Icon { get; set; }

	[Description("图标SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? IconSvg { get; set; }

	[Description("名称")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? Name { get; set; }

	[Description("描述，可选")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
	public string? Description
	{
		get
		{
			return Localization.GetLangI(LocalizationDescription, description, new string[2] { "{id}", ID });
		}
		set
		{
			if (!(description == value))
			{
				description = value;
				Invalidates();
			}
		}
	}

	[Description("详情描述")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationDescription { get; set; }

	[Description("文本")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
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

	[Description("颜色类型")]
	[Category("外观")]
	[DefaultValue(TTypeMini.Primary)]
	public TTypeMini Type { get; set; } = TTypeMini.Primary;


	[Description("填充颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Fill { get; set; }

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

	internal Timeline? PARENT { get; set; }

	internal float pen_w { get; set; } = 3f;


	internal Rectangle rect { get; set; }

	internal Rectangle txt_rect { get; set; }

	internal Rectangle description_rect { get; set; }

	internal Rectangle ico_rect { get; set; }

	public TimelineItem()
	{
	}

	public TimelineItem(string text)
	{
		Text = text;
	}

	public TimelineItem(string text, string description)
	{
		Text = text;
		Description = description;
	}

	public TimelineItem(string text, string description, Image? icon)
	{
		Text = text;
		Description = description;
		Icon = icon;
	}

	public TimelineItem(string text, Image? icon)
	{
		Text = text;
		Icon = icon;
	}

	private void Invalidates()
	{
		if (PARENT != null)
		{
			PARENT.ChangeList();
			((Control)PARENT).Invalidate();
		}
	}

	public override string? ToString()
	{
		return Text;
	}
}
