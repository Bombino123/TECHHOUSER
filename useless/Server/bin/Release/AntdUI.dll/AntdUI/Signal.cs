using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Signal 信号强度")]
[ToolboxItem(true)]
public class Signal : IControl
{
	private Color? fill;

	private int vol;

	private int loading_vol;

	private bool loading;

	private ITask? ThreadLoading;

	private bool styleLine;

	[Description("填充颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Fill
	{
		get
		{
			return fill;
		}
		set
		{
			if (!(fill == value))
			{
				fill = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("满格颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	public Color? FillFully { get; set; }

	[Description("警告颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	public Color? FillWarn { get; set; }

	[Description("危险颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	public Color? FillDanger { get; set; }

	[Description("信号强度")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Value
	{
		get
		{
			return vol;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			else if (value > 5)
			{
				value = 5;
			}
			if (vol != value)
			{
				vol = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("加载状态")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Loading
	{
		get
		{
			return loading;
		}
		set
		{
			if (loading == value)
			{
				return;
			}
			loading = value;
			ThreadLoading?.Dispose();
			if (loading)
			{
				loading_vol = 0;
				bool add = true;
				ThreadLoading = new ITask((Control)(object)this, delegate
				{
					if (add)
					{
						loading_vol += 10;
						if (loading_vol == 100)
						{
							add = false;
						}
					}
					else
					{
						loading_vol -= 10;
						if (loading_vol == 0)
						{
							add = true;
						}
					}
					((Control)this).Invalidate();
					return loading;
				}, 80, delegate
				{
					((Control)this).Invalidate();
				});
			}
			else
			{
				((Control)this).Invalidate();
			}
		}
	}

	[Description("启用线样式")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool StyleLine
	{
		get
		{
			return styleLine;
		}
		set
		{
			if (styleLine != value)
			{
				styleLine = value;
				((Control)this).Invalidate();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadLoading?.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f9: Expected O, but got Unknown
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Expected O, but got Unknown
		//IL_09af: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b6: Expected O, but got Unknown
		//IL_0900: Unknown result type (might be due to invalid IL or missing references)
		//IL_0907: Expected O, but got Unknown
		//IL_072f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0736: Expected O, but got Unknown
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Expected O, but got Unknown
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Expected O, but got Unknown
		//IL_0a8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a92: Expected O, but got Unknown
		//IL_0741: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Expected O, but got Unknown
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Expected O, but got Unknown
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0b84: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8b: Expected O, but got Unknown
		//IL_074b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0752: Expected O, but got Unknown
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Expected O, but got Unknown
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Expected O, but got Unknown
		//IL_0d2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d34: Expected O, but got Unknown
		//IL_0c7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c83: Expected O, but got Unknown
		//IL_09dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e3: Expected O, but got Unknown
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_065c: Expected O, but got Unknown
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Expected O, but got Unknown
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Expected O, but got Unknown
		//IL_0d3b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d42: Expected O, but got Unknown
		//IL_0c8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c91: Expected O, but got Unknown
		//IL_0ab8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abf: Expected O, but got Unknown
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Expected O, but got Unknown
		//IL_0bb1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb8: Expected O, but got Unknown
		//IL_0ac9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad0: Expected O, but got Unknown
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Expected O, but got Unknown
		//IL_0bc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc9: Expected O, but got Unknown
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Expected O, but got Unknown
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return;
		}
		Rectangle rectangle = clientRectangle.PaddingRect(((Control)this).Padding);
		Canvas canvas = e.Graphics.High();
		int num = ((rectangle.Width > rectangle.Height) ? rectangle.Height : rectangle.Width);
		Rectangle rectangle2 = new Rectangle(rectangle.X + (rectangle.Width - num) / 2, rectangle.Y + (rectangle.Height - num) / 2, num, num);
		if (styleLine)
		{
			int num2 = (int)((float)num * 0.12f);
			int num3 = num2 * 2;
			float num4 = ((float)num - (float)num2 * 4f) / 5f;
			int num5 = num3 * 4;
			int num6 = num3 * 3;
			int num7 = num3 * 2;
			int num8 = num3;
			RectangleF rectangleF = new RectangleF(rectangle2.X, rectangle2.Y + num5, num4, rectangle2.Height - num5);
			RectangleF rectangleF2 = new RectangleF((float)(rectangle2.X + num2) + num4, rectangle2.Y + num6, num4, rectangle2.Height - num6);
			RectangleF rectangleF3 = new RectangleF((float)rectangle2.X + ((float)num2 + num4) * 2f, rectangle2.Y + num7, num4, rectangle2.Height - num7);
			RectangleF rectangleF4 = new RectangleF((float)rectangle2.X + ((float)num2 + num4) * 3f, rectangle2.Y + num8, num4, rectangle2.Height - num8);
			RectangleF rectangleF5 = new RectangleF((float)rectangle2.X + ((float)num2 + num4) * 4f, rectangle2.Y, num4, rectangle2.Height);
			if (loading)
			{
				Color color = fill ?? Colour.FillQuaternary.Get("Signal");
				Color color2 = FillFully ?? Colour.Success.Get("Signal");
				GraphicsPath val = new GraphicsPath();
				try
				{
					val.AddRectangle(rectangleF);
					val.AddRectangle(rectangleF2);
					val.AddRectangle(rectangleF3);
					val.AddRectangle(rectangleF4);
					val.AddRectangle(rectangleF5);
					LinearGradientBrush val2 = new LinearGradientBrush(rectangle2, color2, color, 0f);
					try
					{
						ColorBlend val3 = new ColorBlend(3);
						val3.Colors = new Color[3] { color2, color2, color };
						val3.Positions = new float[3]
						{
							0f,
							(float)loading_vol / 100f,
							1f
						};
						val2.InterpolationColors = val3;
						canvas.Fill((Brush)(object)val2, val);
						canvas.Draw(color2, Config.Dpi, val);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else if (vol == 0)
			{
				SolidBrush val4 = new SolidBrush(fill ?? Colour.FillQuaternary.Get("Signal"));
				try
				{
					canvas.Fill((Brush)(object)val4, rectangleF);
					canvas.Fill((Brush)(object)val4, rectangleF2);
					canvas.Fill((Brush)(object)val4, rectangleF3);
					canvas.Fill((Brush)(object)val4, rectangleF4);
					canvas.Fill((Brush)(object)val4, rectangleF5);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			else if (vol == 1)
			{
				SolidBrush val5 = new SolidBrush(fill ?? Colour.FillQuaternary.Get("Signal"));
				try
				{
					SolidBrush val6 = new SolidBrush(FillDanger ?? Colour.Error.Get("Signal"));
					try
					{
						canvas.Fill((Brush)(object)val6, rectangleF);
						canvas.Fill((Brush)(object)val5, rectangleF2);
						canvas.Fill((Brush)(object)val5, rectangleF3);
						canvas.Fill((Brush)(object)val5, rectangleF4);
						canvas.Fill((Brush)(object)val5, rectangleF5);
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
			}
			else if (vol == 2)
			{
				SolidBrush val7 = new SolidBrush(fill ?? Colour.FillQuaternary.Get("Signal"));
				try
				{
					SolidBrush val8 = new SolidBrush(FillDanger ?? Colour.Error.Get("Signal"));
					try
					{
						canvas.Fill((Brush)(object)val8, rectangleF);
						canvas.Fill((Brush)(object)val8, rectangleF2);
						canvas.Fill((Brush)(object)val7, rectangleF3);
						canvas.Fill((Brush)(object)val7, rectangleF4);
						canvas.Fill((Brush)(object)val7, rectangleF5);
					}
					finally
					{
						((IDisposable)val8)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val7)?.Dispose();
				}
			}
			else if (vol == 3)
			{
				SolidBrush val9 = new SolidBrush(fill ?? Colour.FillQuaternary.Get("Signal"));
				try
				{
					SolidBrush val10 = new SolidBrush(FillWarn ?? Colour.Warning.Get("Signal"));
					try
					{
						canvas.Fill((Brush)(object)val10, rectangleF);
						canvas.Fill((Brush)(object)val10, rectangleF2);
						canvas.Fill((Brush)(object)val10, rectangleF3);
						canvas.Fill((Brush)(object)val9, rectangleF4);
						canvas.Fill((Brush)(object)val9, rectangleF5);
					}
					finally
					{
						((IDisposable)val10)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val9)?.Dispose();
				}
			}
			else if (vol == 4)
			{
				SolidBrush val11 = new SolidBrush(fill ?? Colour.FillQuaternary.Get("Signal"));
				try
				{
					SolidBrush val12 = new SolidBrush(FillFully ?? Colour.Success.Get("Signal"));
					try
					{
						canvas.Fill((Brush)(object)val12, rectangleF);
						canvas.Fill((Brush)(object)val12, rectangleF2);
						canvas.Fill((Brush)(object)val12, rectangleF3);
						canvas.Fill((Brush)(object)val12, rectangleF4);
						canvas.Fill((Brush)(object)val11, rectangleF5);
					}
					finally
					{
						((IDisposable)val12)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val11)?.Dispose();
				}
			}
			else
			{
				SolidBrush val13 = new SolidBrush(FillFully ?? Colour.Success.Get("Signal"));
				try
				{
					canvas.Fill((Brush)(object)val13, rectangleF);
					canvas.Fill((Brush)(object)val13, rectangleF2);
					canvas.Fill((Brush)(object)val13, rectangleF3);
					canvas.Fill((Brush)(object)val13, rectangleF4);
					canvas.Fill((Brush)(object)val13, rectangleF5);
				}
				finally
				{
					((IDisposable)val13)?.Dispose();
				}
			}
		}
		else
		{
			Rectangle rectangle3 = new Rectangle(rectangle2.X, rectangle2.Y + rectangle2.Height / 2 / 2, rectangle2.Width, rectangle2.Height);
			if (loading)
			{
				Color color3 = fill ?? Colour.FillQuaternary.Get("Signal");
				Color color4 = FillFully ?? Colour.Success.Get("Signal");
				Pen val14 = new Pen(color4, Config.Dpi);
				try
				{
					LinearGradientBrush val15 = new LinearGradientBrush(rectangle3, color3, color4, 90f);
					try
					{
						ColorBlend val3 = new ColorBlend(2);
						val3.Colors = new Color[3] { color3, color4, color4 };
						val3.Positions = new float[3]
						{
							0f,
							(float)loading_vol / 100f,
							1f
						};
						val15.InterpolationColors = val3;
						canvas.FillPie((Brush)(object)val15, rectangle3, -135f, 90f);
						canvas.DrawPie(val14, rectangle3, -135f, 90f);
					}
					finally
					{
						((IDisposable)val15)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val14)?.Dispose();
				}
			}
			else
			{
				int num9 = (int)((float)num * 0.12f);
				float num10 = rectangle2.Width - num9;
				float num11 = num10 - (float)num9 * 3f;
				float num12 = num11 - (float)num9 * 2f;
				float num13 = (float)rectangle3.Y + (float)num9 / 2f;
				float num14 = num13 + (float)num9 * 1.5f;
				float y = num14 + (float)num9;
				RectangleF rect = new RectangleF((float)rectangle2.X + ((float)rectangle2.Width - num10) / 2f, num13, num10, num10);
				RectangleF rect2 = new RectangleF((float)rectangle2.X + ((float)rectangle2.Width - num11) / 2f, num14, num11, num11);
				RectangleF rectangleF6 = new RectangleF((float)rectangle2.X + ((float)rectangle2.Width - num12) / 2f, y, num12, num12);
				if (vol == 0)
				{
					Pen val16 = new Pen(fill ?? Colour.FillQuaternary.Get("Signal"), (float)num9);
					try
					{
						SolidBrush val17 = new SolidBrush(val16.Color);
						try
						{
							canvas.DrawArc(val16, rect, -135f, 90f);
							canvas.DrawArc(val16, rect2, -135f, 90f);
							canvas.FillPie((Brush)(object)val17, rectangleF6.X, rectangleF6.Y, rectangleF6.Width, rectangleF6.Height, -135f, 90f);
						}
						finally
						{
							((IDisposable)val17)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val16)?.Dispose();
					}
				}
				else if (vol == 1)
				{
					Pen val18 = new Pen(fill ?? Colour.FillQuaternary.Get("Signal"), (float)num9);
					try
					{
						SolidBrush val19 = new SolidBrush(FillDanger ?? Colour.Error.Get("Signal"));
						try
						{
							canvas.DrawArc(val18, rect, -135f, 90f);
							canvas.DrawArc(val18, rect2, -135f, 90f);
							canvas.FillPie((Brush)(object)val19, rectangleF6.X, rectangleF6.Y, rectangleF6.Width, rectangleF6.Height, -135f, 90f);
						}
						finally
						{
							((IDisposable)val19)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val18)?.Dispose();
					}
				}
				else if (vol == 2)
				{
					Pen val20 = new Pen(fill ?? Colour.FillQuaternary.Get("Signal"), (float)num9);
					try
					{
						SolidBrush val21 = new SolidBrush(FillWarn ?? Colour.Warning.Get("Signal"));
						try
						{
							Pen val22 = new Pen(val21.Color, (float)num9);
							try
							{
								canvas.DrawArc(val20, rect, -135f, 90f);
								canvas.DrawArc(val20, rect2, -135f, 90f);
								canvas.FillPie((Brush)(object)val21, rectangleF6.X, rectangleF6.Y, rectangleF6.Width, rectangleF6.Height, -135f, 90f);
							}
							finally
							{
								((IDisposable)val22)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val21)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val20)?.Dispose();
					}
				}
				else if (vol == 3)
				{
					Pen val23 = new Pen(fill ?? Colour.FillQuaternary.Get("Signal"), (float)num9);
					try
					{
						SolidBrush val24 = new SolidBrush(FillWarn ?? Colour.Warning.Get("Signal"));
						try
						{
							Pen val25 = new Pen(val24.Color, (float)num9);
							try
							{
								canvas.DrawArc(val23, rect, -135f, 90f);
								canvas.DrawArc(val25, rect2, -135f, 90f);
								canvas.FillPie((Brush)(object)val24, rectangleF6.X, rectangleF6.Y, rectangleF6.Width, rectangleF6.Height, -135f, 90f);
							}
							finally
							{
								((IDisposable)val25)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val24)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val23)?.Dispose();
					}
				}
				else if (vol == 4)
				{
					Pen val26 = new Pen(FillFully ?? Colour.Success.Get("Signal"), (float)num9);
					try
					{
						SolidBrush val27 = new SolidBrush(val26.Color);
						try
						{
							canvas.DrawArc(val26, rect, -135f, 90f);
							canvas.DrawArc(val26, rect2, -135f, 90f);
							canvas.FillPie((Brush)(object)val27, rectangleF6.X, rectangleF6.Y, rectangleF6.Width, rectangleF6.Height, -135f, 90f);
						}
						finally
						{
							((IDisposable)val27)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val26)?.Dispose();
					}
				}
				else
				{
					Pen val28 = new Pen(FillFully ?? Colour.SuccessActive.Get("Signal"), (float)num9);
					try
					{
						SolidBrush val29 = new SolidBrush(val28.Color);
						try
						{
							canvas.DrawArc(val28, rect, -135f, 90f);
							canvas.DrawArc(val28, rect2, -135f, 90f);
							canvas.FillPie((Brush)(object)val29, rectangleF6.X, rectangleF6.Y, rectangleF6.Width, rectangleF6.Height, -135f, 90f);
						}
						finally
						{
							((IDisposable)val29)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val28)?.Dispose();
					}
				}
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}
}
