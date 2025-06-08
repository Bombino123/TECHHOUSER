using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

internal class TooltipForm : ILayeredFormOpacity, ITooltip
{
	private readonly Control? ocontrol;

	private bool multiline;

	private readonly StringFormat s_c = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private readonly StringFormat s_l = Helper.SF((StringAlignment)1, (StringAlignment)0);

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(6)]
	public int Radius { get; set; } = 6;


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

	public TooltipForm(Control control, string txt, ITooltipConfig component)
	{
		ocontrol = control;
		control.Parent.SetTopMost(((Control)this).Handle);
		((Control)this).Text = txt;
		if (component.Font != null)
		{
			((Control)this).Font = component.Font;
		}
		else if (Config.Font != null)
		{
			((Control)this).Font = Config.Font;
		}
		ArrowSize = component.ArrowSize;
		Radius = component.Radius;
		ArrowAlign = component.ArrowAlign;
		CustomWidth = component.CustomWidth;
		Helper.GDI(delegate(Canvas g)
		{
			SetSize(this.RenderMeasure(g, out multiline));
		});
		Point point = control.PointToScreen(Point.Empty);
		if (component is Tooltip.Config config)
		{
			if (config.Offset is RectangleF rectangleF)
			{
				SetLocation(ArrowAlign.AlignPoint(new Rectangle(point.X + (int)rectangleF.X, point.Y + (int)rectangleF.Y, (int)rectangleF.Width, (int)rectangleF.Height), base.TargetRect.Width, base.TargetRect.Height));
			}
			else if (config.Offset is Rectangle rectangle)
			{
				SetLocation(ArrowAlign.AlignPoint(new Rectangle(point.X + rectangle.X, point.Y + rectangle.Y, rectangle.Width, rectangle.Height), base.TargetRect.Width, base.TargetRect.Height));
			}
			else
			{
				SetLocation(ArrowAlign.AlignPoint(point, control.Size, base.TargetRect.Width, base.TargetRect.Height));
			}
		}
		else
		{
			SetLocation(ArrowAlign.AlignPoint(point, control.Size, base.TargetRect.Width, base.TargetRect.Height));
		}
		control.LostFocus += Control_LostFocus;
		control.MouseLeave += Control_LostFocus;
	}

	public TooltipForm(Control control, Rectangle rect, string txt, ITooltipConfig component)
	{
		ocontrol = control;
		control.SetTopMost(((Control)this).Handle);
		((Control)this).Text = txt;
		if (component.Font != null)
		{
			((Control)this).Font = component.Font;
		}
		else if (Config.Font != null)
		{
			((Control)this).Font = Config.Font;
		}
		ArrowSize = component.ArrowSize;
		Radius = component.Radius;
		ArrowAlign = component.ArrowAlign;
		CustomWidth = component.CustomWidth;
		Helper.GDI(delegate(Canvas g)
		{
			SetSize(this.RenderMeasure(g, out multiline));
		});
		SetLocation(ArrowAlign.AlignPoint(rect, base.TargetRect));
	}

	public void SetText(Rectangle rect, string text)
	{
		((Control)this).Text = text;
		Helper.GDI(delegate(Canvas g)
		{
			SetSize(this.RenderMeasure(g, out multiline));
		});
		SetLocation(ArrowAlign.AlignPoint(rect, base.TargetRect));
		Print();
	}

	private void Control_LostFocus(object? sender, EventArgs e)
	{
		IClose();
	}

	public override Bitmap PrintBit()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using Canvas g = Graphics.FromImage((Image)(object)val).High();
		this.Render(g, targetRectXY, multiline, s_c, s_l);
		return val;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (ocontrol != null)
		{
			ocontrol.LostFocus -= Control_LostFocus;
			ocontrol.MouseLeave -= Control_LostFocus;
		}
	}
}
