using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

[Description("Tooltip 文字提示")]
[ToolboxItem(true)]
public class Tooltip : IControl, ITooltip
{
	public class Config : ITooltipConfig
	{
		public Control Control { get; set; }

		public object? Offset { get; set; }

		public Font? Font { get; set; }

		public string Text { get; set; }

		public int Radius { get; set; } = 6;


		public int ArrowSize { get; set; } = 8;


		public TAlign ArrowAlign { get; set; } = TAlign.Top;


		public int? CustomWidth { get; set; }

		public Config(Control control, string text)
		{
			Font = control.Font;
			Control = control;
			Text = text;
		}
	}

	private string? text;

	private int radius = 6;

	private readonly StringFormat s_c = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private readonly StringFormat s_l = Helper.SF((StringAlignment)1, (StringAlignment)0);

	[Description("文本")]
	[Category("外观")]
	[DefaultValue(null)]
	public override string? Text
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationText, text);
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(6)]
	public int Radius
	{
		get
		{
			return radius;
		}
		set
		{
			if (radius != value)
			{
				radius = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("箭头大小")]
	[Category("外观")]
	[DefaultValue(8)]
	public int ArrowSize { get; set; } = 8;


	[Description("箭头方向")]
	[Category("外观")]
	[DefaultValue(TAlign.Top)]
	public TAlign ArrowAlign { get; set; } = TAlign.Top;


	[Description("自定义宽度")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? CustomWidth { get; set; }

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		Canvas g = e.Graphics.High();
		bool multiline;
		Size maximumSize = (((Control)this).MinimumSize = this.RenderMeasure(g, out multiline));
		((Control)this).MaximumSize = maximumSize;
		this.Render(g, clientRectangle, multiline, s_c, s_l);
		((Control)this).OnPaint(e);
	}

	public static Form? open(Control control, string text, TAlign ArrowAlign = TAlign.Top)
	{
		return open(new Config(control, text)
		{
			ArrowAlign = ArrowAlign
		});
	}

	public static Form? open(Control control, string text, Rectangle rect, TAlign ArrowAlign = TAlign.Top)
	{
		return open(new Config(control, text)
		{
			Offset = rect,
			ArrowAlign = ArrowAlign
		});
	}

	public static Form? open(Config config)
	{
		Config config2 = config;
		if (config2.Control.IsHandleCreated)
		{
			if (config2.Control.InvokeRequired)
			{
				Form form = null;
				config2.Control.Invoke((Delegate)(Action)delegate
				{
					form = open(config2);
				});
				return form;
			}
			TooltipForm tooltipForm = new TooltipForm(config2.Control, config2.Text, config2);
			((Form)tooltipForm).Show((IWin32Window)(object)config2.Control);
			return (Form?)(object)tooltipForm;
		}
		return null;
	}
}
