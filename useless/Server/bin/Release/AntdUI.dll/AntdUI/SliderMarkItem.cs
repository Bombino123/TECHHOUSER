using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class SliderMarkItem
{
	private int _value;

	private Color? fore;

	private string? text;

	[Description("值")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				Invalidates();
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
				Invalidates();
			}
		}
	}

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

	internal Slider? PARENT { get; set; }

	private void Invalidates()
	{
		if (PARENT != null)
		{
			((Control)PARENT).Invalidate();
		}
	}

	public override string ToString()
	{
		return _value + " " + text;
	}
}
