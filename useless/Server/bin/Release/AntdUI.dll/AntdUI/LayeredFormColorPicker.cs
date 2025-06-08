using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

public class LayeredFormColorPicker : ILayeredFormOpacityDown
{
	private class InputRect
	{
		public Input input { get; set; }

		public Rectangle rect { get; set; }

		public Rectangle rect_read { get; set; }

		public InputRect(Input _input, Rectangle _rect_read, int wsize, int wsize2)
		{
			input = _input;
			_input.RECTDIV = (rect = new Rectangle(_rect_read.X - wsize, _rect_read.Y - wsize, _rect_read.Width + wsize2, _rect_read.Height + wsize2));
			rect_read = _rect_read;
		}
	}

	private float Radius = 10f;

	private float Radius2 = 8f;

	private TAlign ArrowAlign;

	private readonly int ArrowSize = 8;

	private int gap = 12;

	private int w = 258;

	private int h = 224;

	private int dot_size = 16;

	private int dot_bor_size = 2;

	private int line_h = 8;

	private int panel_color = 28;

	private int btn_size = 24;

	private int offy;

	private Color Value;

	private Color ValueNAlpha;

	private Color ValueHue;

	private Action<Color> action;

	private TColorMode mode;

	private PointF[]? rect_arrow;

	private bool AllowClear;

	private bool isinput = true;

	private bool hover_btn;

	private Rectangle rect_btn;

	private InputRect[] inputs;

	private Bitmap bmp_dot_12;

	private Rectangle rect_color;

	private bool down_colors;

	private Point point_colors = Point.Empty;

	private Rectangle rect_colors_big;

	private Rectangle rect_colors;

	private Bitmap? bmp_colors;

	private Dictionary<string, Color>? bmp_colors_mouse;

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

	private Bitmap? shadow_temp;

	public LayeredFormColorPicker(ColorPicker control, Rectangle rect_read, Action<Color> _action)
	{
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Expected O, but got Unknown
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Expected O, but got Unknown
		//IL_0cc9: Unknown result type (might be due to invalid IL or missing references)
		((Control)control).Parent.SetTopMost(((Control)this).Handle);
		AllowClear = control.AllowClear;
		((Control)this).Font = ((Control)control).Font;
		mode = control.Mode;
		base.MessageCloseMouseLeave = control.Trigger == Trigger.Hover;
		color_alpha = (Value = control.Value);
		ValueNAlpha = Color.FromArgb(255, Value);
		HSV hSV = ValueNAlpha.ToHSV();
		hSV.s = (hSV.v = 1f);
		ValueHue = hSV.HSVToColor();
		Radius = (float)control.Radius * Config.Dpi;
		Radius2 = Radius * 0.75f;
		PARENT = (Control?)(object)control;
		action = _action;
		int num2 = 160;
		if (Config.Dpi != 1f)
		{
			num2 = (int)((float)num2 * Config.Dpi);
			gap = (int)((float)gap * Config.Dpi);
			dot_size = (int)((float)dot_size * Config.Dpi);
			dot_bor_size = (int)((float)dot_bor_size * Config.Dpi);
			btn_size = (int)((float)btn_size * Config.Dpi);
			line_h = (int)((float)line_h * Config.Dpi);
			panel_color = (int)((float)panel_color * Config.Dpi);
			w = (int)((float)w * Config.Dpi);
			h = (int)((float)h * Config.Dpi);
		}
		if (control.Mode == TColorMode.Rgb)
		{
			if (control.DisabledAlpha)
			{
				w = Helper.GDI(delegate(Canvas g)
				{
					Size size2 = g.MeasureString("255%", ((Control)this).Font);
					return (int)Math.Ceiling((float)(size2.Width + size2.Height) * 3.4f);
				});
			}
			else
			{
				w = Helper.GDI(delegate(Canvas g)
				{
					Size size = g.MeasureString("255%", ((Control)this).Font);
					return (int)Math.Ceiling((float)(size.Width + size.Height) * 4.4f);
				});
			}
			int num3 = (int)Math.Ceiling((float)w * 0.62f);
			int num4 = num3 - num2;
			num2 = num3;
			h += num4;
		}
		int num5 = 10;
		if (AllowClear)
		{
			rect_btn = new Rectangle(10 + w - gap - btn_size, num5 + gap, btn_size, btn_size);
			offy = btn_size + line_h + line_h / 2;
			num5 += offy;
			h += offy;
		}
		rect_colors = new Rectangle(10 + gap, num5 + gap, w - gap * 2, num2);
		rect_color = new Rectangle(10 + gap + (w - gap * 2) - panel_color, rect_colors.Bottom + gap, panel_color, panel_color);
		rect_hue = new Rectangle(10 + gap, rect_colors.Bottom + gap, w - gap * 3 - panel_color, line_h);
		int num6 = rect_hue.Bottom + gap;
		int num7 = line_h / 2;
		if (control.DisabledAlpha)
		{
			rect_alpha = (rect_alpha_big = new Rectangle(-1, -1, 0, 0));
			rect_hue.Offset(0, line_h + num7 / 2);
		}
		else
		{
			rect_alpha = new Rectangle(rect_hue.X, rect_hue.Bottom + gap, rect_hue.Width, line_h);
			rect_alpha_big = new Rectangle(rect_alpha.X - num7, rect_alpha.Y - num7, rect_alpha.Width + line_h, rect_alpha.Height + line_h);
		}
		rect_colors_big = new Rectangle(rect_colors.X - num7, rect_colors.Y - num7, rect_colors.Width + line_h, rect_colors.Height + line_h);
		rect_hue_big = new Rectangle(rect_hue.X - num7, rect_hue.Y - num7, rect_hue.Width + line_h, rect_hue.Height + line_h);
		bmp_dot_12 = new Bitmap(gap + 12, gap + 12);
		using (Canvas canvas = Graphics.FromImage((Image)(object)bmp_dot_12).High())
		{
			SolidBrush val = new SolidBrush(Colour.BgBase.Get("ColorPicker"));
			try
			{
				_ = (float)(((Image)bmp_dot_12).Height - gap) / 2f;
				RectangleF rect = new RectangleF(6f, 6f, ((Image)bmp_dot_12).Height - 12, ((Image)bmp_dot_12).Height - 12);
				canvas.FillEllipse(Brushes.Black, rect);
				Helper.Blur(bmp_dot_12, 6);
				canvas.CompositingMode = (CompositingMode)1;
				canvas.FillEllipse(Brushes.Transparent, rect);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		h += panel_color + gap;
		int width = w + 20;
		int height = h + 20;
		SetSize(width, height);
		rect_arrow = CLocation(((Control)control).PointToScreen(Point.Empty), control.Placement, control.DropDownArrow, ArrowSize, 10, width, height, rect_read, ref Inverted, ref ArrowAlign, Collision: true);
		((Form)this).Location = base.TargetRect.Location;
		((Form)this).Size = base.TargetRect.Size;
		Rectangle rectangle = new Rectangle(rect_colors_big.X + 4, num6 + line_h + gap, rect_colors_big.Width - 8, panel_color);
		int num8 = (int)(4f * Config.Dpi);
		int num9 = num8 * 2;
		TColorMode tColorMode = control.Mode;
		if (tColorMode != 0 && tColorMode == TColorMode.Rgb)
		{
			if (control.DisabledAlpha)
			{
				int num10 = rectangle.Width / 3 - num8;
				Rectangle rect_read2 = new Rectangle(rectangle.X, rectangle.Y, num10, rectangle.Height);
				Rectangle rect_read3 = new Rectangle(rectangle.X + num10 + num8, rectangle.Y, num10, rectangle.Height);
				Rectangle rect_read4 = new Rectangle(rectangle.X + num10 * 2 + num9, rectangle.Y, num10, rectangle.Height);
				InputNumber obj = new InputNumber
				{
					ShowControl = false,
					PrefixText = "R"
				};
				((Control)obj).Location = rect_read2.Location;
				((Control)obj).Size = rect_read2.Size;
				obj.Value = Value.R;
				obj.Maximum = 255;
				InputNumber inputNumber = obj;
				inputNumber.ValueChanged += delegate(object a, DecimalEventArgs e)
				{
					if (isinput)
					{
						ChangeColor(Color.FromArgb(Value.A, (int)e.Value, Value.G, Value.B));
					}
				};
				InputNumber obj2 = new InputNumber
				{
					ShowControl = false,
					PrefixText = "G"
				};
				((Control)obj2).Location = rect_read3.Location;
				((Control)obj2).Size = rect_read3.Size;
				obj2.Value = Value.G;
				obj2.Maximum = 255;
				InputNumber inputNumber2 = obj2;
				inputNumber2.ValueChanged += delegate(object a, DecimalEventArgs e)
				{
					if (isinput)
					{
						ChangeColor(Color.FromArgb(Value.A, Value.R, (int)e.Value, Value.B));
					}
				};
				InputNumber obj3 = new InputNumber
				{
					ShowControl = false,
					PrefixText = "B"
				};
				((Control)obj3).Location = rect_read4.Location;
				((Control)obj3).Size = rect_read4.Size;
				obj3.Value = Value.B;
				obj3.Maximum = 255;
				InputNumber inputNumber3 = obj3;
				inputNumber3.ValueChanged += delegate(object a, DecimalEventArgs e)
				{
					if (isinput)
					{
						ChangeColor(Color.FromArgb(Value.A, Value.R, Value.G, (int)e.Value));
					}
				};
				inputs = new InputRect[3]
				{
					new InputRect(inputNumber, rect_read2, num8, num9),
					new InputRect(inputNumber2, rect_read3, num8, num9),
					new InputRect(inputNumber3, rect_read4, num8, num9)
				};
				inputNumber.TakePaint = (inputNumber2.TakePaint = (inputNumber3.TakePaint = delegate
				{
					if (!RunAnimation)
					{
						Print(fore: true);
					}
				}));
				((Control)this).Controls.Add((Control)(object)inputNumber);
				((Control)this).Controls.Add((Control)(object)inputNumber2);
				((Control)this).Controls.Add((Control)(object)inputNumber3);
				return;
			}
			int num11 = rectangle.Width / 4 - num8;
			Rectangle rect_read5 = new Rectangle(rectangle.X, rectangle.Y, num11, rectangle.Height);
			Rectangle rect_read6 = new Rectangle(rectangle.X + num11 + num8, rectangle.Y, num11, rectangle.Height);
			Rectangle rect_read7 = new Rectangle(rectangle.X + num11 * 2 + num9, rectangle.Y, num11, rectangle.Height);
			Rectangle rect_read8 = new Rectangle(rectangle.X + num11 * 3 + num8 * 3, rectangle.Y, num11, rectangle.Height);
			InputNumber obj4 = new InputNumber
			{
				ShowControl = false,
				PrefixText = "R"
			};
			((Control)obj4).Location = rect_read5.Location;
			((Control)obj4).Size = rect_read5.Size;
			obj4.Value = Value.R;
			obj4.Maximum = 255;
			InputNumber inputNumber4 = obj4;
			inputNumber4.ValueChanged += delegate(object a, DecimalEventArgs e)
			{
				if (isinput)
				{
					ChangeColor(Color.FromArgb(Value.A, (int)e.Value, Value.G, Value.B));
				}
			};
			InputNumber obj5 = new InputNumber
			{
				ShowControl = false,
				PrefixText = "G"
			};
			((Control)obj5).Location = rect_read6.Location;
			((Control)obj5).Size = rect_read6.Size;
			obj5.Value = Value.G;
			obj5.Maximum = 255;
			InputNumber inputNumber5 = obj5;
			inputNumber5.ValueChanged += delegate(object a, DecimalEventArgs e)
			{
				if (isinput)
				{
					ChangeColor(Color.FromArgb(Value.A, Value.R, (int)e.Value, Value.B));
				}
			};
			InputNumber obj6 = new InputNumber
			{
				ShowControl = false,
				PrefixText = "B"
			};
			((Control)obj6).Location = rect_read7.Location;
			((Control)obj6).Size = rect_read7.Size;
			obj6.Value = Value.B;
			obj6.Maximum = 255;
			InputNumber inputNumber6 = obj6;
			inputNumber6.ValueChanged += delegate(object a, DecimalEventArgs e)
			{
				if (isinput)
				{
					ChangeColor(Color.FromArgb(Value.A, Value.R, Value.G, (int)e.Value));
				}
			};
			InputNumber obj7 = new InputNumber
			{
				ShowControl = false,
				SuffixText = "%"
			};
			((Control)obj7).Location = rect_read8.Location;
			((Control)obj7).Size = rect_read8.Size;
			obj7.Value = (int)((float)(int)Value.A / 255f * 100f);
			obj7.Maximum = 100;
			InputNumber inputNumber7 = obj7;
			inputNumber7.ValueChanged += delegate(object s, DecimalEventArgs e)
			{
				if (isinput)
				{
					ChangeColor(Color.FromArgb((int)(255f * ((float)(int)e.Value / 100f)), Value.R, Value.G, Value.B), a: true);
				}
			};
			inputs = new InputRect[4]
			{
				new InputRect(inputNumber4, rect_read5, num8, num9),
				new InputRect(inputNumber5, rect_read6, num8, num9),
				new InputRect(inputNumber6, rect_read7, num8, num9),
				new InputRect(inputNumber7, rect_read8, num8, num9)
			};
			inputNumber4.TakePaint = (inputNumber5.TakePaint = (inputNumber6.TakePaint = (inputNumber7.TakePaint = delegate
			{
				if (!RunAnimation)
				{
					Print(fore: true);
				}
			})));
			((Control)this).Controls.Add((Control)(object)inputNumber4);
			((Control)this).Controls.Add((Control)(object)inputNumber5);
			((Control)this).Controls.Add((Control)(object)inputNumber6);
			((Control)this).Controls.Add((Control)(object)inputNumber7);
			return;
		}
		Input obj8 = new Input
		{
			RECTDIV = rectangle
		};
		((Control)obj8).Padding = new Padding(rect_colors_big.X + 4, 0, rect_colors_big.X + 4, 0);
		((Control)obj8).Location = new Point(0, rectangle.Y);
		((Control)obj8).Size = new Size(w + 20, rectangle.Height);
		obj8.TextAlign = (HorizontalAlignment)2;
		((Control)obj8).Text = "#" + Value.ToHex();
		Input input = obj8;
		input.TakePaint = delegate
		{
			if (!RunAnimation)
			{
				Print(fore: true);
			}
		};
		((Control)input).TextChanged += delegate
		{
			if (isinput)
			{
				ChangeColor(((Control)input).Text.ToColor());
			}
		};
		((Control)this).Controls.Add((Control)(object)input);
		inputs = new InputRect[1]
		{
			new InputRect(input, rectangle, num8, num9)
		};
	}

	private void ChangeColor(Color color, bool a = false)
	{
		color_alpha = (Value = color);
		ValueNAlpha = Color.FromArgb(255, Value);
		HSV hSV = ValueNAlpha.ToHSV();
		float s = (hSV.v = 1f);
		hSV.s = s;
		ValueHue = hSV.HSVToColor();
		action(Value);
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
		if (a)
		{
			Bitmap? obj4 = bmp_alpha_read;
			if (obj4 != null)
			{
				((Image)obj4).Dispose();
			}
			bmp_alpha_read = null;
		}
		Print(fore: true);
	}

	public override void LoadOK()
	{
		((Control)this).BeginInvoke((Delegate)(Action)delegate
		{
			((Form)this).Location = base.TargetRect.Location;
			((Form)this).Size = base.TargetRect.Size;
			Input input = new Input();
			((Control)input).Dock = (DockStyle)2;
			((Control)input).Size = new Size(0, 30);
			Input input2 = input;
			((Control)this).Controls.Add((Control)(object)input2);
		});
		base.LoadOK();
	}

	private void SetValue()
	{
		isinput = false;
		TColorMode tColorMode = mode;
		if (tColorMode != 0 && tColorMode == TColorMode.Rgb)
		{
			((InputNumber)inputs[0].input).Value = Value.R;
			((InputNumber)inputs[1].input).Value = Value.G;
			((InputNumber)inputs[2].input).Value = Value.B;
			if (inputs.Length > 3)
			{
				((InputNumber)inputs[3].input).Value = (int)((float)(int)Value.A / 255f * 100f);
			}
		}
		else
		{
			((Control)inputs[0].input).Text = "#" + Value.ToHex();
		}
		action(Value);
		isinput = true;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (RunAnimation)
		{
			return;
		}
		if ((int)e.Button == 1048576)
		{
			if (rect_colors_big.Contains(e.Location))
			{
				if (bmp_colors_mouse != null)
				{
					Color colors = GetColors(e.X, e.Y, bmp_colors_mouse);
					color_alpha = (Value = Color.FromArgb(Value.A, colors));
					ValueNAlpha = Color.FromArgb(255, Value);
					SetValue();
					Bitmap? obj = bmp_alpha;
					if (obj != null)
					{
						((Image)obj).Dispose();
					}
					bmp_alpha = null;
					Print(fore: true);
					down_colors = true;
				}
			}
			else if (rect_hue_big.Contains(e.Location))
			{
				if (bmp_hue != null)
				{
					point_hue = e.X - 10 - gap;
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
					color_alpha = (Value = Color.FromArgb(Value.A, hSV2.HSVToColor()));
					ValueNAlpha = Color.FromArgb(255, Value);
					SetValue();
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
					Print(fore: true);
					down_hue = true;
				}
			}
			else if (rect_alpha_big.Contains(e.Location))
			{
				if (bmp_alpha_read != null)
				{
					point_alpha = e.X - 10 - gap;
					if (point_alpha < 0)
					{
						point_alpha = 0;
					}
					else if (point_alpha > ((Image)bmp_alpha_read).Width - 1)
					{
						point_alpha = ((Image)bmp_alpha_read).Width - 1;
					}
					color_alpha = (Value = Color.FromArgb(bmp_alpha_read.GetPixel(point_alpha, 1).A, ValueNAlpha));
					SetValue();
					Print(fore: true);
					down_alpha = true;
				}
			}
			else if (AllowClear && rect_btn.Contains(e.Location) && PARENT is ColorPicker { HasValue: not false } colorPicker)
			{
				colorPicker.ClearValue();
				Print();
			}
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		if (AllowClear)
		{
			bool flag = rect_btn.Contains(e.X, e.Y);
			if (flag != hover_btn)
			{
				hover_btn = flag;
				Print();
			}
		}
		if (down_colors && bmp_colors_mouse != null)
		{
			Color colors = GetColors(e.X, e.Y, bmp_colors_mouse);
			color_alpha = (Value = Color.FromArgb(Value.A, colors));
			ValueNAlpha = Color.FromArgb(255, Value);
			SetValue();
			Bitmap? obj = bmp_alpha;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			bmp_alpha = null;
			Print(fore: true);
		}
		else if (down_hue && bmp_hue != null)
		{
			point_hue = e.X - 10 - gap;
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
			color_alpha = (Value = Color.FromArgb(Value.A, hSV2.HSVToColor()));
			ValueNAlpha = Color.FromArgb(255, Value);
			SetValue();
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
			Print(fore: true);
		}
		else if (down_alpha && bmp_alpha_read != null)
		{
			point_alpha = e.X - 10 - gap;
			if (point_alpha < 0)
			{
				point_alpha = 0;
			}
			else if (point_alpha > ((Image)bmp_alpha_read).Width - 1)
			{
				point_alpha = ((Image)bmp_alpha_read).Width - 1;
			}
			color_alpha = (Value = Color.FromArgb(bmp_alpha_read.GetPixel(point_alpha, 1).A, ValueNAlpha));
			SetValue();
			Print(fore: true);
		}
		((Control)this).OnMouseMove(e);
	}

	private Color GetColors(int x, int y, Dictionary<string, Color> dir)
	{
		point_colors = new Point(x - 10 - gap, y - 10 - offy - gap);
		int num = rect_colors.Width - 1;
		int num2 = rect_colors.Height - 1;
		if (point_colors.X < 0)
		{
			point_colors.X = 0;
		}
		else if (point_colors.X > num)
		{
			point_colors.X = num;
		}
		if (point_colors.Y < 0)
		{
			point_colors.Y = 0;
		}
		else if (point_colors.Y > num2)
		{
			point_colors.Y = num2;
		}
		return dir[point_colors.X + "_" + point_colors.Y];
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (!RunAnimation)
		{
			if (down_colors)
			{
				down_colors = false;
			}
			if (down_hue)
			{
				down_hue = false;
			}
			if (down_alpha)
			{
				down_alpha = false;
			}
			((Control)this).OnMouseUp(e);
		}
	}

	public override Bitmap PrintBit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Expected O, but got Unknown
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Expected O, but got Unknown
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Expected O, but got Unknown
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Expected O, but got Unknown
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Expected O, but got Unknown
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rectangle = new Rectangle(10, 10, targetRectXY.Width - 20, targetRectXY.Height - 20);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using Canvas canvas = Graphics.FromImage((Image)(object)val).High();
		SolidBrush val2 = new SolidBrush(Colour.BgElevated.Get("ColorPicker"));
		try
		{
			GraphicsPath val3 = rectangle.RoundPath(Radius);
			try
			{
				DrawShadow(canvas, targetRectXY);
				canvas.Fill((Brush)(object)val2, val3);
				if (ArrowAlign != 0)
				{
					if (rect_arrow == null)
					{
						canvas.FillPolygon((Brush)(object)val2, ArrowAlign.AlignLines(ArrowSize, targetRectXY, rectangle));
					}
					else
					{
						canvas.FillPolygon((Brush)(object)val2, rect_arrow);
					}
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			if (AllowClear)
			{
				GraphicsPath val4 = rect_btn.RoundPath(Radius2);
				try
				{
					canvas.SetClip(val4);
					Pen val5 = new Pen(Color.FromArgb(245, 34, 45), (float)rect_btn.Height * 0.12f);
					try
					{
						canvas.DrawLine(val5, new Point(rect_btn.X, rect_btn.Bottom), new Point(rect_btn.Right, rect_btn.Y));
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
					canvas.ResetClip();
					canvas.Draw(hover_btn ? Colour.BorderColor.Get("ColorPicker") : Colour.Split.Get("ColorPicker"), Config.Dpi, val4);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			if (bmp_colors == null)
			{
				bmp_colors = new Bitmap(rect_colors.Width, rect_colors.Height);
				using (Canvas g = Graphics.FromImage((Image)(object)bmp_colors).High())
				{
					PaintColors(g, new Rectangle(0, 0, ((Image)bmp_colors).Width, ((Image)bmp_colors).Height));
				}
				bmp_colors_mouse = GetColorsPoint(bmp_colors);
			}
			canvas.Image(bmp_colors, rect_colors);
			GraphicsPath val6 = rect_colors.RoundPath(Radius);
			try
			{
				val6.AddRectangle(new Rectangle(rect_colors.X - 1, rect_colors.Y - 1, rect_colors.Width + 2, rect_colors.Height + 2));
				canvas.Fill((Brush)(object)val2, val6);
			}
			finally
			{
				((IDisposable)val6)?.Dispose();
			}
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
			GraphicsPath val7 = rect_hue.RoundPath(rect_hue.Height);
			try
			{
				val7.AddRectangle(new Rectangle(rect_hue.X - 1, rect_hue.Y - 1, rect_hue.Width + 2, rect_hue.Height + 2));
				canvas.Fill((Brush)(object)val2, val7);
			}
			finally
			{
				((IDisposable)val7)?.Dispose();
			}
			if (rect_alpha.Width > 0)
			{
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
				GraphicsPath val8 = rect_alpha.RoundPath(rect_alpha.Height);
				try
				{
					val8.AddRectangle(new Rectangle(rect_alpha.X - 1, rect_alpha.Y - 1, rect_alpha.Width + 2, rect_alpha.Height + 2));
					canvas.Fill((Brush)(object)val2, val8);
				}
				finally
				{
					((IDisposable)val8)?.Dispose();
				}
			}
			SolidBrush val9 = new SolidBrush(Value);
			try
			{
				SolidBrush val10 = new SolidBrush(ValueHue);
				try
				{
					Pen val11 = new Pen(Colour.BgBase.Get("ColorPicker"), (float)dot_bor_size);
					try
					{
						Rectangle rect = new Rectangle(rect_colors.X + point_colors.X - dot_size / 2, rect_colors.Y + point_colors.Y - dot_size / 2, dot_size, dot_size);
						canvas.FillEllipse((Brush)(object)val9, rect);
						canvas.DrawEllipse(val11, rect);
						Rectangle rect2 = new Rectangle(rect_hue.X + point_hue - gap / 2, rect_hue.Y + rect_hue.Height / 2 - gap / 2, gap, gap);
						canvas.Image(bmp_dot_12, new Rectangle(rect_hue.X + point_hue - ((Image)bmp_dot_12).Height / 2, rect_hue.Y + (rect_hue.Height - ((Image)bmp_dot_12).Height) / 2, ((Image)bmp_dot_12).Width, ((Image)bmp_dot_12).Height));
						canvas.FillEllipse((Brush)(object)val10, rect2);
						canvas.DrawEllipse(val11, rect2);
						if (rect_alpha.Width > 0)
						{
							val9.Color = color_alpha;
							Rectangle rect3 = new Rectangle(rect_alpha.X + point_alpha - gap / 2, rect_alpha.Y + rect_alpha.Height / 2 - gap / 2, gap, gap);
							canvas.Image(bmp_dot_12, new Rectangle(rect_alpha.X + point_alpha - ((Image)bmp_dot_12).Height / 2, rect_alpha.Y + (rect_alpha.Height - ((Image)bmp_dot_12).Height) / 2, ((Image)bmp_dot_12).Width, ((Image)bmp_dot_12).Height));
							canvas.FillEllipse((Brush)(object)val9, rect3);
							canvas.DrawEllipse(val11, rect3);
						}
						GraphicsPath val12 = rect_color.RoundPath(Radius);
						try
						{
							canvas.Fill((Brush)(object)val9, val12);
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
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		InputRect[] array = inputs;
		foreach (InputRect inputRect in array)
		{
			inputRect.input.IPaint(canvas, inputRect.rect, inputRect.rect_read);
		}
		return val;
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

	private Dictionary<string, Color> GetColorsPoint(Bitmap bmp_colors)
	{
		int width = ((Image)bmp_colors).Width;
		int height = ((Image)bmp_colors).Height;
		Dictionary<string, Color> dictionary = new Dictionary<string, Color>(width * height);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				try
				{
					Color pixel = bmp_colors.GetPixel(i, j);
					dictionary.Add(i + "_" + j, pixel);
					if (pixel == ValueNAlpha)
					{
						point_colors = new Point(i, j);
					}
				}
				catch
				{
					return dictionary;
				}
			}
		}
		return dictionary;
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
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		if (add)
		{
			SolidBrush val = new SolidBrush(Colour.FillSecondary.Get("ColorPicker"));
			try
			{
				int num = rect.Height / 2;
				int i = 0;
				bool flag = false;
				for (; i < rect.Width; i += num)
				{
					flag = !flag;
					g.Fill((Brush)(object)val, new Rectangle(i, (!flag) ? num : 0, num, num));
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
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

	private void DrawShadow(Canvas g, Rectangle rect)
	{
		if (!Config.ShadowEnabled)
		{
			return;
		}
		if (shadow_temp == null)
		{
			Bitmap? obj = shadow_temp;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			GraphicsPath val = new Rectangle(10, 10, rect.Width - 20, rect.Height - 20).RoundPath(Radius);
			try
			{
				shadow_temp = val.PaintShadow(rect.Width, rect.Height);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Image(shadow_temp, rect, 0.2f);
	}

	public void IProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		InputRect[] array = inputs;
		foreach (InputRect inputRect in array)
		{
			if (((Control)inputRect.input).Focused)
			{
				inputRect.input.IProcessCmdKey(ref msg, keyData);
			}
		}
	}

	public void IKeyPress(KeyPressEventArgs e)
	{
		InputRect[] array = inputs;
		foreach (InputRect inputRect in array)
		{
			if (((Control)inputRect.input).Focused)
			{
				inputRect.input.IKeyPress(e);
				break;
			}
		}
	}
}
