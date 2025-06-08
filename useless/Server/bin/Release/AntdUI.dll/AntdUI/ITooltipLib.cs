using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI;

internal static class ITooltipLib
{
	public static Size RenderMeasure(this ITooltip core, Canvas g, out bool multiline)
	{
		multiline = core.Text.Contains("\n");
		int num = (int)Math.Ceiling(20f * Config.Dpi);
		Size size = g.MeasureString(core.Text, core.Font);
		if (core.CustomWidth.HasValue)
		{
			int num2 = (int)Math.Ceiling((float)core.CustomWidth.Value * Config.Dpi);
			if (size.Width > num2)
			{
				size = g.MeasureString(core.Text, core.Font, num2);
				multiline = true;
			}
		}
		if (core.ArrowAlign == TAlign.None)
		{
			return new Size(size.Width + num, size.Height + num);
		}
		if (core.ArrowAlign == TAlign.Bottom || core.ArrowAlign == TAlign.BL || core.ArrowAlign == TAlign.BR || core.ArrowAlign == TAlign.Top || core.ArrowAlign == TAlign.TL || core.ArrowAlign == TAlign.TR)
		{
			return new Size(size.Width + num, size.Height + num + core.ArrowSize);
		}
		return new Size(size.Width + num + core.ArrowSize, size.Height + num);
	}

	public static void Render(this ITooltip core, Canvas g, Rectangle rect, bool multiline, StringFormat s_c, StringFormat s_l)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		int num = (int)Math.Ceiling(5f * Config.Dpi);
		int num2 = num * 2;
		int padding = num2 * 2;
		SolidBrush val = new SolidBrush((Config.Mode == TMode.Dark) ? Color.FromArgb(66, 66, 66) : Color.FromArgb(38, 38, 38));
		try
		{
			if (core.ArrowAlign == TAlign.None)
			{
				Rectangle rect2 = new Rectangle(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10);
				GraphicsPath val2 = rect2.RoundPath(core.Radius);
				try
				{
					core.DrawShadow(g, rect, rect2, 3, val2);
					g.Fill((Brush)(object)val, val2);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				RenderText(core, g, rect2, multiline, num2, padding, s_c, s_l);
				return;
			}
			Rectangle rect3 = core.ArrowAlign.AlignMini() switch
			{
				TAlignMini.Top => new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - core.ArrowSize), 
				TAlignMini.Bottom => new Rectangle(rect.X, rect.Y + core.ArrowSize, rect.Width, rect.Height - core.ArrowSize), 
				TAlignMini.Left => new Rectangle(rect.X, rect.Y, rect.Width - core.ArrowSize, rect.Height), 
				_ => new Rectangle(rect.X + core.ArrowSize, rect.Y, rect.Width - core.ArrowSize, rect.Height), 
			};
			Rectangle rectangle = new Rectangle(rect3.X + num, rect3.Y + num, rect3.Width - num2, rect3.Height - num2);
			GraphicsPath val3 = rectangle.RoundPath(core.Radius);
			try
			{
				core.DrawShadow(g, rect, rectangle, 3, val3);
				g.Fill((Brush)(object)val, val3);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			g.FillPolygon((Brush)(object)val, core.ArrowAlign.AlignLines(core.ArrowSize, rect, rectangle));
			RenderText(core, g, rect3, multiline, num2, padding, s_c, s_l);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void RenderText(ITooltip core, Canvas g, Rectangle rect, bool multiline, int padding, int padding2, StringFormat s_c, StringFormat s_l)
	{
		if (multiline)
		{
			g.String(core.Text, core.Font, Brushes.White, new Rectangle(rect.X + padding, rect.Y + padding, rect.Width - padding2, rect.Height - padding2), s_l);
		}
		else
		{
			g.String(core.Text, core.Font, Brushes.White, rect, s_c);
		}
	}

	private static void DrawShadow(this ITooltip core, Canvas _g, Rectangle brect, Rectangle rect, int size, GraphicsPath path2)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		Bitmap val = new Bitmap(brect.Width, brect.Height);
		try
		{
			using (Canvas canvas = Graphics.FromImage((Image)(object)val).HighLay())
			{
				int num = size * 2;
				GraphicsPath val2 = new Rectangle(rect.X - size, rect.Y - size + 2, rect.Width + num, rect.Height + num).RoundPath(core.Radius);
				try
				{
					val2.AddPath(path2, false);
					PathGradientBrush val3 = new PathGradientBrush(val2);
					try
					{
						val3.CenterColor = Color.Black;
						val3.SurroundColors = new Color[1] { Color.Transparent };
						canvas.Fill((Brush)(object)val3, val2);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			_g.Image(val, brect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
