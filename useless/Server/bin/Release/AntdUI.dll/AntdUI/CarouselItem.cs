using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class CarouselItem
{
	private Image? img;

	[Description("ID")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? ID { get; set; }

	[Description("图片")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Img
	{
		get
		{
			return img;
		}
		set
		{
			if (img != value)
			{
				img = value;
				Carousel? pARENT = PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	internal Carousel? PARENT { get; set; }

	[Description("用户定义数据")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? Tag { get; set; }
}
