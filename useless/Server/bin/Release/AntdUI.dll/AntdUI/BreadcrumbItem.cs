using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class BreadcrumbItem
{
	private Image? icon;

	private string? iconsvg;

	private string? text;

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

	[Description("用户定义数据")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	internal bool Hover { get; set; }

	internal Rectangle Rect { get; set; }

	internal Rectangle RectImg { get; set; }

	internal Rectangle RectText { get; set; }

	internal Breadcrumb? PARENT { get; set; }

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
