using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI.Design;

internal class FrmColorEditor : UserControl
{
	private Action<Color> action;

	private static readonly Color[] colors = new Color[18]
	{
		"#f44336".ToColor(),
		"#e91e63".ToColor(),
		"#9c27b0".ToColor(),
		"#673ab7".ToColor(),
		"#3f51b5".ToColor(),
		"#2196f3".ToColor(),
		"#03a9f4".ToColor(),
		"#00bcd4".ToColor(),
		"#009688".ToColor(),
		"#4caf50".ToColor(),
		"#cddc39".ToColor(),
		"#ffeb3b".ToColor(),
		"#ffc107".ToColor(),
		"#ff9800".ToColor(),
		"#ff5722".ToColor(),
		"#795548".ToColor(),
		"#9e9e9e".ToColor(),
		"#607d8b".ToColor()
	};

	private Rectangle[] rects_colors = new Rectangle[0];

	public Color Value;

	private Color ValueNAlpha;

	private Color ValueHue;

	private Bitmap? bmp_dot_12;

	private int gap = 12;

	private bool down_colors;

	private Point point_colors = Point.Empty;

	private Rectangle rect_colors_big;

	private Rectangle rect_colors;

	private Bitmap? bmp_colors;

	private bool down_hue;

	private int point_hue;

	private Rectangle rect_hue_big;

	private Rectangle rect_hue;

	private Bitmap? bmp_hue;

	private bool down_alpha;

	private int point_alpha;

	private Rectangle rect_alpha_big;

	private Rectangle rect_alpha;

	private Bitmap? bmp_alpha;

	private Bitmap? bmp_alpha_read;

	private Color color_alpha = Color.White;

	private IContainer components;

	private Input input1;

	public FrmColorEditor(object? val)
	{
		((Control)this).SetStyle((ControlStyles)204802, true);
		((Control)this).UpdateStyles();
		InitializeComponent();
		if (val is Color value)
		{
			color_alpha = (Value = value);
		}
		else
		{
			color_alpha = (Value = Colour.Primary.Get());
		}
		ValueNAlpha = Color.FromArgb(255, Value);
		HSV hSV = ValueNAlpha.ToHSV();
		float s = (hSV.v = 1f);
		hSV.s = s;
		ValueHue = hSV.HSVToColor();
		((Control)input1).Text = Value.ToHex();
		((Control)input1).TextChanged += input1_TextChanged;
		action = delegate(Color val)
		{
			((Control)input1).Text = val.ToHex();
		};
	}

	protected override void OnLoad(EventArgs e)
	{
		Helper.DpiAuto(Config.Dpi, (Control)(object)this);
		((UserControl)this).OnLoad(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Expected O, but got Unknown
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Expected O, but got Unknown
		//IL_054f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Expected O, but got Unknown
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d4: Expected O, but got Unknown
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Expected O, but got Unknown
		//IL_06af: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Expected O, but got Unknown
		//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c7: Expected O, but got Unknown
		//IL_09f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f8: Expected O, but got Unknown
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		Canvas canvas = e.Graphics.High();
		SolidBrush val = new SolidBrush(((Control)this).BackColor);
		try
		{
			int width = clientRectangle.Width;
			int num = 16;
			int num2 = 2;
			int num3 = 8;
			int num4 = 28;
			int height = (int)((float)width * 0.58f);
			if (Config.Dpi != 1f)
			{
				gap = (int)(12f * Config.Dpi);
				num = (int)((float)num * Config.Dpi);
				num2 = (int)((float)num2 * Config.Dpi);
				num3 = (int)((float)num3 * Config.Dpi);
				num4 = (int)((float)num4 * Config.Dpi);
			}
			if (bmp_dot_12 == null)
			{
				bmp_dot_12 = new Bitmap(gap + 12, gap + 12);
				using Canvas canvas2 = Graphics.FromImage((Image)(object)bmp_dot_12).High();
				SolidBrush val2 = new SolidBrush(Colour.BgBase.Get());
				try
				{
					_ = (float)(((Image)bmp_dot_12).Height - gap) / 2f;
					RectangleF rect = new RectangleF(6f, 6f, ((Image)bmp_dot_12).Height - 12, ((Image)bmp_dot_12).Height - 12);
					canvas2.FillEllipse(Brushes.Black, rect);
					Helper.Blur(bmp_dot_12, 6);
					canvas2.CompositingMode = (CompositingMode)1;
					canvas2.FillEllipse(Brushes.Transparent, rect);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			rect_colors = new Rectangle(0, 0, width, height);
			Rectangle rect2 = new Rectangle(gap + (width - gap * 2) - num4, rect_colors.Bottom + gap, num4, num4);
			rect_hue = new Rectangle(gap, rect_colors.Bottom + gap, width - gap * 3 - num4, num3);
			rect_alpha = new Rectangle(rect_hue.X, rect_hue.Bottom + gap, rect_hue.Width, num3);
			int num5 = num3 / 2;
			rect_colors_big = new Rectangle(rect_colors.X - num5, rect_colors.Y - num5, rect_colors.Width + num3, rect_colors.Height + num3);
			rect_hue_big = new Rectangle(rect_hue.X - num5, rect_hue.Y - num5, rect_hue.Width + num3, rect_hue.Height + num3);
			rect_alpha_big = new Rectangle(rect_alpha.X - num5, rect_alpha.Y - num5, rect_alpha.Width + num3, rect_alpha.Height + num3);
			if (bmp_colors != null && ((Image)bmp_colors).Width != rect_colors.Width)
			{
				((Image)bmp_colors).Dispose();
				bmp_colors = null;
			}
			if (bmp_hue != null && ((Image)bmp_hue).Width != rect_hue.Width)
			{
				((Image)bmp_hue).Dispose();
				bmp_hue = null;
			}
			if (bmp_alpha_read != null && ((Image)bmp_alpha_read).Width != rect_alpha.Width)
			{
				((Image)bmp_alpha_read).Dispose();
				bmp_alpha_read = null;
			}
			if (bmp_alpha != null && ((Image)bmp_alpha).Width != rect_alpha.Width)
			{
				((Image)bmp_alpha).Dispose();
				bmp_alpha = null;
			}
			if (bmp_colors == null)
			{
				bmp_colors = new Bitmap(rect_colors.Width, rect_colors.Height);
				using (Canvas g = Graphics.FromImage((Image)(object)bmp_colors).High())
				{
					PaintColors(g, new Rectangle(0, 0, ((Image)bmp_colors).Width, ((Image)bmp_colors).Height));
				}
				GetColorsPoint(bmp_colors);
			}
			canvas.Image(bmp_colors, rect_colors);
			if (bmp_hue == null)
			{
				bmp_hue = new Bitmap(rect_hue.Width, rect_hue.Height);
				using (Canvas g2 = Graphics.FromImage((Image)(object)bmp_hue).High())
				{
					PaintHue(g2, new Rectangle(0, 0, ((Image)bmp_hue).Width, ((Image)bmp_hue).Height));
				}
				GetHuePoint(bmp_hue);
			}
			canvas.Image(bmp_hue, rect_hue);
			GraphicsPath val3 = rect_hue.RoundPath(rect_hue.Height);
			try
			{
				val3.AddRectangle(new Rectangle(rect_hue.X - 1, rect_hue.Y - 1, rect_hue.Width + 2, rect_hue.Height + 2));
				canvas.Fill((Brush)(object)val, val3);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			if (bmp_alpha_read == null)
			{
				bmp_alpha_read = new Bitmap(rect_alpha.Width, rect_alpha.Height);
				using (Canvas g3 = Graphics.FromImage((Image)(object)bmp_alpha_read).High())
				{
					PaintAlpha(g3, new Rectangle(0, 0, ((Image)bmp_alpha_read).Width, ((Image)bmp_alpha_read).Height), add: false);
				}
				GetAlphaPoint(bmp_alpha_read);
			}
			if (bmp_alpha == null)
			{
				bmp_alpha = new Bitmap(rect_alpha.Width, rect_alpha.Height);
				using Canvas g4 = Graphics.FromImage((Image)(object)bmp_alpha).High();
				PaintAlpha(g4, new Rectangle(0, 0, ((Image)bmp_alpha).Width, ((Image)bmp_alpha).Height), add: true);
			}
			canvas.Image(bmp_alpha, rect_alpha);
			GraphicsPath val4 = rect_alpha.RoundPath(rect_alpha.Height);
			try
			{
				val4.AddRectangle(new Rectangle(rect_alpha.X - 1, rect_alpha.Y - 1, rect_alpha.Width + 2, rect_alpha.Height + 2));
				canvas.Fill((Brush)(object)val, val4);
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			SolidBrush val5 = new SolidBrush(Value);
			try
			{
				SolidBrush val6 = new SolidBrush(ValueHue);
				try
				{
					Pen val7 = new Pen(Colour.BgBase.Get(), (float)num2);
					try
					{
						Rectangle rect3 = new Rectangle(rect_colors.X + point_colors.X - num / 2, rect_colors.Y + point_colors.Y - num / 2, num, num);
						canvas.FillEllipse((Brush)(object)val5, rect3);
						canvas.DrawEllipse(val7, rect3);
						Rectangle rect4 = new Rectangle(rect_hue.X + point_hue - gap / 2, rect_hue.Y + rect_hue.Height / 2 - gap / 2, gap, gap);
						canvas.Image(bmp_dot_12, new Rectangle(rect_hue.X + point_hue - ((Image)bmp_dot_12).Height / 2, rect_hue.Y + (rect_hue.Height - ((Image)bmp_dot_12).Height) / 2, ((Image)bmp_dot_12).Width, ((Image)bmp_dot_12).Height));
						canvas.FillEllipse((Brush)(object)val6, rect4);
						canvas.DrawEllipse(val7, rect4);
						val5.Color = color_alpha;
						Rectangle rect5 = new Rectangle(rect_alpha.X + point_alpha - gap / 2, rect_alpha.Y + rect_alpha.Height / 2 - gap / 2, gap, gap);
						canvas.Image(bmp_dot_12, new Rectangle(rect_alpha.X + point_alpha - ((Image)bmp_dot_12).Height / 2, rect_alpha.Y + (rect_alpha.Height - ((Image)bmp_dot_12).Height) / 2, ((Image)bmp_dot_12).Width, ((Image)bmp_dot_12).Height));
						canvas.FillEllipse((Brush)(object)val5, rect5);
						canvas.DrawEllipse(val7, rect5);
						canvas.FillEllipse((Brush)(object)val5, rect2);
					}
					finally
					{
						((IDisposable)val7)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
			int num6 = gap * 2;
			int num7 = rect_alpha.Bottom + num4 + num3 * 2 + gap;
			int num8 = (width - num6) / num6 - 1;
			int num9 = 0;
			int num10 = (int)Math.Ceiling((float)input1.WaveSize * Config.Dpi);
			((Control)input1).Left = gap - num10;
			((Control)input1).Height = num4 + gap;
			((Control)input1).Width = width - num6 + num10 * 2;
			((Control)input1).Top = rect_alpha.Bottom + num3;
			List<Rectangle> list = new List<Rectangle>(colors.Length);
			Color[] array = colors;
			foreach (Color color in array)
			{
				Rectangle rectangle = new Rectangle(rect_alpha.X + num6 * num9, num7, gap, gap);
				list.Add(rectangle);
				SolidBrush val8 = new SolidBrush(color);
				try
				{
					canvas.Fill((Brush)(object)val8, rectangle);
				}
				finally
				{
					((IDisposable)val8)?.Dispose();
				}
				num9++;
				if (num9 > num8)
				{
					num9 = 0;
					num7 += num6;
				}
			}
			rects_colors = list.ToArray();
			((Control)this).Height = num7;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		((Control)this).OnPaint(e);
	}

	private void PaintColors(Canvas g, Rectangle rect)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(ValueHue);
		try
		{
			g.Fill((Brush)(object)val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		RectangleF rectangleF = new RectangleF(rect.X, rect.Y, (float)rect.Width - 2f, rect.Height);
		RectangleF rectangleF2 = new RectangleF(rect.X, (float)rect.Y + 2f, rect.Width, (float)rect.Height - 4f);
		LinearGradientBrush val2 = new LinearGradientBrush(rectangleF, Color.White, Color.Transparent, 0f);
		try
		{
			g.Fill((Brush)(object)val2, rectangleF);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		LinearGradientBrush val3 = new LinearGradientBrush(rectangleF2, Color.Transparent, Color.Black, 90f);
		try
		{
			g.Fill((Brush)(object)val3, rectangleF2);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		SolidBrush val4 = new SolidBrush(Color.Black);
		try
		{
			g.Fill((Brush)(object)val4, new RectangleF(rect.X, (float)rect.Height - 2f, rect.Width, 2f));
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
	}

	private void GetColorsPoint(Bitmap bmp_colors)
	{
		for (int i = 0; i < ((Image)bmp_colors).Width; i++)
		{
			for (int j = 0; j < ((Image)bmp_colors).Height; j++)
			{
				if (bmp_colors.GetPixel(i, j) == ValueNAlpha)
				{
					point_colors = new Point(i, j);
					return;
				}
			}
		}
	}

	private void PaintHue(Canvas g, Rectangle rect)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Expected O, but got Unknown
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Expected O, but got Unknown
		int num = (rect.Width - 4) / 6;
		Rectangle rectangle = new Rectangle(2, 0, num, rect.Height);
		Rectangle rectangle2 = new Rectangle(rectangle.X + num, 0, num, rect.Height);
		Rectangle rectangle3 = new Rectangle(rectangle.X + num * 2, 0, num, rect.Height);
		Rectangle rectangle4 = new Rectangle(rectangle.X + num * 3, 0, num, rect.Height);
		Rectangle rectangle5 = new Rectangle(rectangle.X + num * 4, 0, num, rect.Height);
		Rectangle rectangle6 = new Rectangle(rectangle.X + num * 5, 0, num, rect.Height);
		SolidBrush val = new SolidBrush(Color.FromArgb(255, 0, 0));
		try
		{
			g.Fill((Brush)(object)val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		LinearGradientBrush val2 = new LinearGradientBrush(rectangle, Color.FromArgb(255, 0, 0), Color.FromArgb(255, 255, 0), 0f);
		try
		{
			g.Fill((Brush)(object)val2, rectangle);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		LinearGradientBrush val3 = new LinearGradientBrush(rectangle2, Color.FromArgb(255, 255, 0), Color.FromArgb(0, 255, 0), 0f);
		try
		{
			g.Fill((Brush)(object)val3, rectangle2);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		LinearGradientBrush val4 = new LinearGradientBrush(rectangle3, Color.FromArgb(0, 255, 0), Color.FromArgb(0, 255, 255), 0f);
		try
		{
			g.Fill((Brush)(object)val4, rectangle3);
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
		LinearGradientBrush val5 = new LinearGradientBrush(rectangle4, Color.FromArgb(0, 255, 255), Color.FromArgb(0, 0, 255), 0f);
		try
		{
			g.Fill((Brush)(object)val5, rectangle4);
		}
		finally
		{
			((IDisposable)val5)?.Dispose();
		}
		LinearGradientBrush val6 = new LinearGradientBrush(rectangle5, Color.FromArgb(0, 0, 255), Color.FromArgb(255, 0, 255), 0f);
		try
		{
			g.Fill((Brush)(object)val6, rectangle5);
		}
		finally
		{
			((IDisposable)val6)?.Dispose();
		}
		LinearGradientBrush val7 = new LinearGradientBrush(rectangle6, Color.FromArgb(255, 0, 255), Color.FromArgb(255, 0, 0), 0f);
		try
		{
			g.Fill((Brush)(object)val7, rectangle6);
		}
		finally
		{
			((IDisposable)val7)?.Dispose();
		}
	}

	private void GetHuePoint(Bitmap bmp_hue)
	{
		int num = ((Image)bmp_hue).Height / 2;
		List<Color> list = new List<Color>();
		for (int i = 0; i < ((Image)bmp_hue).Width; i++)
		{
			Color pixel = bmp_hue.GetPixel(i, num);
			if (pixel == ValueHue)
			{
				point_hue = i;
				return;
			}
			list.Add(pixel);
		}
		point_hue = find_i(list, ValueHue);
	}

	private int find_i(List<Color> cols, Color x)
	{
		int num = int.MaxValue;
		int num2 = num;
		for (int i = 0; i < cols.Count; i++)
		{
			int num3 = x.R - cols[i].R;
			int num4 = x.G - cols[i].G;
			int num5 = x.B - cols[i].B;
			int num6 = num3 * num3 + num4 * num4 + num5 * num5;
			if (num6 < num2)
			{
				num2 = num6;
				num = i;
			}
		}
		return num;
	}

	private void PaintAlpha(Canvas g, Rectangle rect, bool add)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (add)
		{
			int num = rect.Height / 2;
			int i = 0;
			bool flag = false;
			for (; i < rect.Width; i += num)
			{
				flag = !flag;
				SolidBrush val = new SolidBrush(Colour.FillSecondary.Get());
				try
				{
					g.Fill((Brush)(object)val, new Rectangle(i, (!flag) ? num : 0, num, num));
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		rect.Offset(1, 0);
		LinearGradientBrush val2 = new LinearGradientBrush(rect, Color.Transparent, ValueNAlpha, 0f);
		try
		{
			g.Fill((Brush)(object)val2, rect);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		SolidBrush val3 = new SolidBrush(ValueNAlpha);
		try
		{
			g.Fill((Brush)(object)val3, new Rectangle(rect.Width - 1, 0, 4, rect.Height));
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private void GetAlphaPoint(Bitmap bmp_alpha)
	{
		int num = ((Image)bmp_alpha).Height / 2;
		List<Color> list = new List<Color>();
		for (int i = 0; i < ((Image)bmp_alpha).Width; i++)
		{
			Color pixel = bmp_alpha.GetPixel(i, num);
			if (pixel.A == Value.A)
			{
				point_alpha = i;
				return;
			}
			list.Add(pixel);
		}
		point_alpha = find_i(list, ValueNAlpha);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576)
		{
			if (rect_colors_big.Contains(e.Location))
			{
				if (bmp_colors != null)
				{
					point_colors = e.Location;
					if (point_colors.X < 0)
					{
						point_colors.X = 0;
					}
					else if (point_colors.X > ((Image)bmp_colors).Width - 1)
					{
						point_colors.X = ((Image)bmp_colors).Width - 1;
					}
					if (point_colors.Y < 0)
					{
						point_colors.Y = 0;
					}
					else if (point_colors.Y > ((Image)bmp_colors).Height - 1)
					{
						point_colors.Y = ((Image)bmp_colors).Height - 1;
					}
					color_alpha = (Value = bmp_colors.GetPixel(point_colors.X, point_colors.Y));
					ValueNAlpha = Color.FromArgb(255, Value);
					action(Value);
					Bitmap? obj = bmp_alpha;
					if (obj != null)
					{
						((Image)obj).Dispose();
					}
					bmp_alpha = null;
					((Control)this).Invalidate();
					down_colors = true;
				}
			}
			else if (rect_hue_big.Contains(e.Location))
			{
				if (bmp_hue != null)
				{
					point_hue = e.X - gap;
					if (point_hue < 0)
					{
						point_hue = 0;
					}
					else if (point_hue > ((Image)bmp_hue).Width - 1)
					{
						point_hue = ((Image)bmp_hue).Width - 1;
					}
					ValueHue = bmp_hue.GetPixel(point_hue, 1);
					HSV hSV = ValueHue.ToHSV();
					HSV hSV2 = Value.ToHSV();
					hSV2.h = hSV.h;
					color_alpha = (Value = hSV2.HSVToColor());
					ValueNAlpha = Color.FromArgb(255, Value);
					action(Value);
					Bitmap? obj2 = bmp_colors;
					if (obj2 != null)
					{
						((Image)obj2).Dispose();
					}
					bmp_colors = null;
					Bitmap? obj3 = bmp_alpha;
					if (obj3 != null)
					{
						((Image)obj3).Dispose();
					}
					bmp_alpha = null;
					((Control)this).Invalidate();
					down_hue = true;
				}
			}
			else if (rect_alpha_big.Contains(e.Location) && bmp_alpha_read != null)
			{
				point_alpha = e.X - gap;
				if (point_alpha < 0)
				{
					point_alpha = 0;
				}
				else if (point_alpha > ((Image)bmp_alpha_read).Width - 1)
				{
					point_alpha = ((Image)bmp_alpha_read).Width - 1;
				}
				color_alpha = (Value = Color.FromArgb(bmp_alpha_read.GetPixel(point_alpha, 1).A, ValueNAlpha));
				action(Value);
				((Control)this).Invalidate();
				down_alpha = true;
			}
		}
		((UserControl)this).OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (down_colors && bmp_colors != null)
		{
			point_colors = e.Location;
			if (point_colors.X < 0)
			{
				point_colors.X = 0;
			}
			else if (point_colors.X > ((Image)bmp_colors).Width - 1)
			{
				point_colors.X = ((Image)bmp_colors).Width - 1;
			}
			if (point_colors.Y < 0)
			{
				point_colors.Y = 0;
			}
			else if (point_colors.Y > ((Image)bmp_colors).Height - 1)
			{
				point_colors.Y = ((Image)bmp_colors).Height - 1;
			}
			color_alpha = (Value = bmp_colors.GetPixel(point_colors.X, point_colors.Y));
			ValueNAlpha = Color.FromArgb(255, Value);
			action(Value);
			Bitmap? obj = bmp_alpha;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			bmp_alpha = null;
			((Control)this).Invalidate();
		}
		else if (down_hue && bmp_hue != null)
		{
			point_hue = e.X - gap;
			if (point_hue < 0)
			{
				point_hue = 0;
			}
			else if (point_hue > ((Image)bmp_hue).Width - 1)
			{
				point_hue = ((Image)bmp_hue).Width - 1;
			}
			ValueHue = bmp_hue.GetPixel(point_hue, 1);
			HSV hSV = ValueHue.ToHSV();
			HSV hSV2 = Value.ToHSV();
			hSV2.h = hSV.h;
			color_alpha = (Value = hSV2.HSVToColor());
			ValueNAlpha = Color.FromArgb(255, Value);
			action(Value);
			Bitmap? obj2 = bmp_colors;
			if (obj2 != null)
			{
				((Image)obj2).Dispose();
			}
			bmp_colors = null;
			Bitmap? obj3 = bmp_alpha;
			if (obj3 != null)
			{
				((Image)obj3).Dispose();
			}
			bmp_alpha = null;
			((Control)this).Invalidate();
		}
		else if (down_alpha && bmp_alpha_read != null)
		{
			point_alpha = e.X - gap;
			if (point_alpha < 0)
			{
				point_alpha = 0;
			}
			else if (point_alpha > ((Image)bmp_alpha_read).Width - 1)
			{
				point_alpha = ((Image)bmp_alpha_read).Width - 1;
			}
			color_alpha = (Value = Color.FromArgb(bmp_alpha_read.GetPixel(point_alpha, 1).A, ValueNAlpha));
			action(Value);
			((Control)this).Invalidate();
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		if (down_colors)
		{
			down_colors = false;
			return;
		}
		if (down_hue)
		{
			down_hue = false;
			return;
		}
		if (down_alpha)
		{
			down_alpha = false;
			return;
		}
		if ((int)e.Button == 1048576)
		{
			for (int i = 0; i < rects_colors.Length; i++)
			{
				if (rects_colors[i].Contains(e.Location))
				{
					action(colors[i]);
					return;
				}
			}
		}
		((Control)this).OnMouseUp(e);
	}

	private void input1_TextChanged(object sender, EventArgs e)
	{
		Bitmap? obj = bmp_colors;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		bmp_colors = null;
		Bitmap? obj2 = bmp_hue;
		if (obj2 != null)
		{
			((Image)obj2).Dispose();
		}
		bmp_hue = null;
		Bitmap? obj3 = bmp_alpha;
		if (obj3 != null)
		{
			((Image)obj3).Dispose();
		}
		bmp_alpha = null;
		color_alpha = (Value = ((Control)input1).Text.ToColor());
		ValueNAlpha = Color.FromArgb(255, Value);
		HSV hSV = ValueNAlpha.ToHSV();
		float s = (hSV.v = 1f);
		hSV.s = s;
		ValueHue = hSV.HSVToColor();
		((Control)this).Invalidate();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((ContainerControl)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		input1 = new Input();
		((Control)this).SuspendLayout();
		((Control)input1).Anchor = (AnchorStyles)0;
		((Control)input1).Location = new Point(43, 174);
		((Control)input1).Name = "input1";
		((Control)input1).Size = new Size(144, 50);
		((Control)input1).TabIndex = 1;
		((Control)this).Controls.Add((Control)(object)input1);
		((Control)this).MinimumSize = new Size(240, 280);
		((Control)this).Name = "FrmColorEditor";
		((Control)this).Size = new Size(240, 280);
		((Control)this).ResumeLayout(false);
	}
}
