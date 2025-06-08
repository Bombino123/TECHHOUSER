using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class SpinCore : IDisposable
{
	private ITask? thread;

	private float LineWidth = 6f;

	private float LineAngle;

	private int prog_size;

	private readonly StringFormat s_f = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	private bool lnull;

	public void Clear()
	{
		prog_size = 0;
	}

	public void Start(IControl control)
	{
		IControl control2 = control;
		bool ProgState = false;
		thread = new ITask((Control)(object)control2, delegate
		{
			if (lnull)
			{
				LineAngle = LineAngle.Calculate(2f);
			}
			else if (ProgState)
			{
				LineAngle = LineAngle.Calculate(9f);
				LineWidth = LineWidth.Calculate(0.6f);
				if (LineWidth > 75f)
				{
					ProgState = false;
				}
			}
			else
			{
				LineAngle = LineAngle.Calculate(9.6f);
				LineWidth = LineWidth.Calculate(-0.6f);
				if (LineWidth < 6f)
				{
					ProgState = true;
				}
			}
			if (LineAngle >= 360f)
			{
				LineAngle = 0f;
			}
			((Control)control2).Invalidate();
			return true;
		}, 10);
	}

	public void Start(ILayeredForm control)
	{
		ILayeredForm control2 = control;
		bool ProgState = false;
		thread = new ITask((Control)(object)control2, delegate
		{
			if (lnull)
			{
				LineAngle = LineAngle.Calculate(2f);
			}
			else if (ProgState)
			{
				LineAngle = LineAngle.Calculate(9f);
				LineWidth = LineWidth.Calculate(0.6f);
				if (LineWidth > 75f)
				{
					ProgState = false;
				}
			}
			else
			{
				LineAngle = LineAngle.Calculate(9.6f);
				LineWidth = LineWidth.Calculate(-0.6f);
				if (LineWidth < 6f)
				{
					ProgState = true;
				}
			}
			if (LineAngle >= 360f)
			{
				LineAngle = 0f;
			}
			control2.Print();
			return true;
		}, 10);
	}

	public void Paint(Canvas g, Rectangle rect, Spin.Config config, Control control)
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		Font font = config.Font ?? control.Font;
		if (prog_size == 0)
		{
			prog_size = g.MeasureString(config.Text ?? "龍Qq", font).Height;
		}
		int num = (int)((float)prog_size * 1.6f);
		int num2 = (int)((float)prog_size * 0.2f);
		int num3 = num / 2;
		Rectangle rectangle = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + (rect.Height - num) / 2, num, num);
		if (config.Text != null)
		{
			int bottom = rectangle.Bottom;
			rectangle.Offset(0, -num3);
			g.String(config.Text, font, config.Fore ?? Colour.Primary.Get("Spin"), new Rectangle(rect.X, bottom, rect.Width, prog_size), s_f);
		}
		g.DrawEllipse(Colour.Fill.Get("Spin"), num2, rectangle);
		Pen val = new Pen(config.Color ?? Colour.Primary.Get("Spin"), (float)num2);
		try
		{
			LineCap startCap = (LineCap)2;
			val.EndCap = (LineCap)2;
			val.StartCap = startCap;
			if (config.Value.HasValue)
			{
				lnull = true;
				g.DrawArc(val, rectangle, LineAngle, config.Value.Value * 360f);
			}
			else
			{
				lnull = false;
				g.DrawArc(val, rectangle, LineAngle, LineWidth * 3.6f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Dispose()
	{
		thread?.Dispose();
	}
}
