using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Battery 电量")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
public class Battery : IControl
{
	private Color? back;

	private Color? fore;

	private int radius = 4;

	private int dotsize = 8;

	private int _value;

	private Color fillfully = Color.FromArgb(0, 210, 121);

	private StringFormat c = Helper.SF((StringAlignment)1, (StringAlignment)1);

	[Description("原装背景颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "Transparent")]
	public Color OriginalBackColor
	{
		get
		{
			return ((Control)this).BackColor;
		}
		set
		{
			((Control)this).BackColor = value;
		}
	}

	[Description("背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackColor
	{
		get
		{
			return back;
		}
		set
		{
			if (!(back == value))
			{
				back = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackColor");
			}
		}
	}

	[Description("文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeColor
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
				((Control)this).Invalidate();
				OnPropertyChanged("ForeColor");
			}
		}
	}

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(4)]
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
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("点大小")]
	[Category("外观")]
	[DefaultValue(8)]
	public int DotSize
	{
		get
		{
			return dotsize;
		}
		set
		{
			if (dotsize != value)
			{
				dotsize = value;
				((Control)this).Invalidate();
				OnPropertyChanged("DotSize");
			}
		}
	}

	[Description("进度条")]
	[Category("数据")]
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
				if (value < 0)
				{
					value = 0;
				}
				else if (value > 100)
				{
					value = 100;
				}
				_value = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Value");
			}
		}
	}

	[Description("显示")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool ShowText { get; set; } = true;


	[Description("满电颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "0, 210, 121")]
	public Color FillFully
	{
		get
		{
			return fillfully;
		}
		set
		{
			if (!(fillfully == value))
			{
				fillfully = value;
				((Control)this).Invalidate();
				OnPropertyChanged("FillFully");
			}
		}
	}

	[Description("警告电量颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "250, 173, 20")]
	public Color FillWarn { get; set; } = Color.FromArgb(250, 173, 20);


	[Description("危险电量颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "255, 77, 79")]
	public Color FillDanger { get; set; } = Color.FromArgb(255, 77, 79);


	[Description("警告电量阈值")]
	[Category("外观")]
	[DefaultValue(30)]
	public int ValueWarn { get; set; } = 30;


	[Description("危险电量阈值")]
	[Category("外观")]
	[DefaultValue(20)]
	public int ValueDanger { get; set; } = 20;


	public Battery()
	{
		((Control)this).BackColor = Color.Transparent;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Expected O, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Expected O, but got Unknown
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		Canvas canvas = e.Graphics.High();
		Size size = canvas.MeasureString("100%", ((Control)this).Font);
		Rectangle rect = new Rectangle((clientRectangle.Width - size.Width) / 2, (clientRectangle.Height - size.Height) / 2, size.Width, size.Height);
		float num = (float)radius * Config.Dpi;
		GraphicsPath val = rect.RoundPath(num);
		try
		{
			if (_value >= 100)
			{
				SolidBrush val2 = new SolidBrush(fillfully);
				try
				{
					canvas.Fill((Brush)(object)val2, val);
					if (dotsize > 0)
					{
						float num2 = (float)dotsize * Config.Dpi;
						GraphicsPath val3 = new RectangleF(rect.Right, (float)rect.Top + ((float)rect.Height - num2) / 2f, num2 / 2f, num2).RoundPath(num / 2f, TL: false, TR: true, BR: true, BL: false);
						try
						{
							canvas.Fill((Brush)(object)val2, val3);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				if (ShowText)
				{
					SolidBrush val4 = new SolidBrush(fore ?? Colour.Text.Get("Battery"));
					try
					{
						canvas.String("100%", ((Control)this).Font, (Brush)(object)val4, rect, c);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
			}
			else
			{
				SolidBrush val5 = new SolidBrush(back ?? Colour.FillSecondary.Get("Battery"));
				try
				{
					canvas.Fill((Brush)(object)val5, val);
					if (dotsize > 0)
					{
						float num3 = (float)dotsize * Config.Dpi;
						GraphicsPath val6 = new RectangleF(rect.Right, (float)rect.Top + ((float)rect.Height - num3) / 2f, num3 / 2f, num3).RoundPath(num / 2f, TL: false, TR: true, BR: true, BL: false);
						try
						{
							canvas.Fill((Brush)(object)val5, val6);
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
				if (_value > 0)
				{
					Bitmap val7 = new Bitmap(clientRectangle.Width, clientRectangle.Height);
					try
					{
						using (Canvas canvas2 = Graphics.FromImage((Image)(object)val7).High())
						{
							Color color = ((_value > ValueWarn) ? fillfully : ((_value <= ValueDanger) ? FillDanger : FillWarn));
							canvas2.Fill(color, val);
							float num4 = (float)rect.Width * ((float)_value / 100f);
							canvas2.CompositingMode = (CompositingMode)1;
							canvas2.Fill(Brushes.Transparent, new RectangleF((float)rect.X + num4, 0f, rect.Width, ((Image)val7).Height));
						}
						canvas.Image(val7, clientRectangle);
					}
					finally
					{
						((IDisposable)val7)?.Dispose();
					}
				}
				if (ShowText)
				{
					SolidBrush val8 = new SolidBrush(fore ?? Colour.Text.Get("Battery"));
					try
					{
						canvas.String(_value + "%", ((Control)this).Font, (Brush)(object)val8, rect, c);
					}
					finally
					{
						((IDisposable)val8)?.Dispose();
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}
}
