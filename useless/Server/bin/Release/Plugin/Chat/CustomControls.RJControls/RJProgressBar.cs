using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CustomControls.RJControls;

internal class RJProgressBar : ProgressBar
{
	private Color channelColor = Color.LightSteelBlue;

	private Color sliderColor = Color.RoyalBlue;

	private Color foreBackColor = Color.RoyalBlue;

	private int channelHeight = 6;

	private int sliderHeight = 6;

	private TextPosition showValue = TextPosition.Right;

	private string symbolBefore = "";

	private string symbolAfter = "";

	private bool showMaximun;

	private bool paintedBack;

	private bool stopPainting;

	[Category("RJ Code Advance")]
	public Color ChannelColor
	{
		get
		{
			return channelColor;
		}
		set
		{
			channelColor = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public Color SliderColor
	{
		get
		{
			return sliderColor;
		}
		set
		{
			sliderColor = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public Color ForeBackColor
	{
		get
		{
			return foreBackColor;
		}
		set
		{
			foreBackColor = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public int ChannelHeight
	{
		get
		{
			return channelHeight;
		}
		set
		{
			channelHeight = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public int SliderHeight
	{
		get
		{
			return sliderHeight;
		}
		set
		{
			sliderHeight = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public TextPosition ShowValue
	{
		get
		{
			return showValue;
		}
		set
		{
			showValue = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public string SymbolBefore
	{
		get
		{
			return symbolBefore;
		}
		set
		{
			symbolBefore = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public string SymbolAfter
	{
		get
		{
			return symbolAfter;
		}
		set
		{
			symbolAfter = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public bool ShowMaximun
	{
		get
		{
			return showMaximun;
		}
		set
		{
			showMaximun = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	[Browsable(true)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	public override Font Font
	{
		get
		{
			return ((ProgressBar)this).Font;
		}
		set
		{
			((ProgressBar)this).Font = value;
		}
	}

	[Category("RJ Code Advance")]
	public override Color ForeColor
	{
		get
		{
			return ((Control)this).ForeColor;
		}
		set
		{
			((Control)this).ForeColor = value;
		}
	}

	public RJProgressBar()
	{
		((Control)this).SetStyle((ControlStyles)2, true);
		((Control)this).ForeColor = Color.White;
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if (stopPainting)
		{
			return;
		}
		if (!paintedBack)
		{
			Graphics graphics = pevent.Graphics;
			Rectangle rectangle = new Rectangle(0, 0, ((Control)this).Width, ChannelHeight);
			SolidBrush val = new SolidBrush(channelColor);
			try
			{
				if (channelHeight >= sliderHeight)
				{
					rectangle.Y = ((Control)this).Height - channelHeight;
				}
				else
				{
					rectangle.Y = ((Control)this).Height - (channelHeight + sliderHeight) / 2;
				}
				graphics.Clear(((Control)this).Parent.BackColor);
				graphics.FillRectangle((Brush)(object)val, rectangle);
				if (!((Component)this).DesignMode)
				{
					paintedBack = true;
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (((ProgressBar)this).Value == ((ProgressBar)this).Maximum || ((ProgressBar)this).Value == ((ProgressBar)this).Minimum)
		{
			paintedBack = false;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		if (!stopPainting)
		{
			Graphics graphics = e.Graphics;
			double num = ((double)((ProgressBar)this).Value - (double)((ProgressBar)this).Minimum) / ((double)((ProgressBar)this).Maximum - (double)((ProgressBar)this).Minimum);
			int num2 = (int)((double)((Control)this).Width * num);
			Rectangle rectangle = new Rectangle(0, 0, num2, sliderHeight);
			SolidBrush val = new SolidBrush(sliderColor);
			try
			{
				if (sliderHeight >= channelHeight)
				{
					rectangle.Y = ((Control)this).Height - sliderHeight;
				}
				else
				{
					rectangle.Y = ((Control)this).Height - (sliderHeight + channelHeight) / 2;
				}
				if (num2 > 1)
				{
					graphics.FillRectangle((Brush)(object)val, rectangle);
				}
				if (showValue != TextPosition.None)
				{
					DrawValueText(graphics, num2, rectangle);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (((ProgressBar)this).Value == ((ProgressBar)this).Maximum)
		{
			stopPainting = true;
		}
		else
		{
			stopPainting = false;
		}
	}

	private void DrawValueText(Graphics graph, int sliderWidth, Rectangle rectSlider)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		string text = symbolBefore + ((ProgressBar)this).Value + symbolAfter;
		if (showMaximun)
		{
			text = text + "/" + symbolBefore + ((ProgressBar)this).Maximum + symbolAfter;
		}
		Size size = TextRenderer.MeasureText(text, ((Control)this).Font);
		Rectangle rectangle = new Rectangle(0, 0, size.Width, size.Height + 2);
		SolidBrush val = new SolidBrush(((Control)this).ForeColor);
		try
		{
			SolidBrush val2 = new SolidBrush(foreBackColor);
			try
			{
				StringFormat val3 = new StringFormat();
				try
				{
					switch (showValue)
					{
					case TextPosition.Left:
						rectangle.X = 0;
						val3.Alignment = (StringAlignment)0;
						break;
					case TextPosition.Right:
						rectangle.X = ((Control)this).Width - size.Width;
						val3.Alignment = (StringAlignment)2;
						break;
					case TextPosition.Center:
						rectangle.X = (((Control)this).Width - size.Width) / 2;
						val3.Alignment = (StringAlignment)1;
						break;
					case TextPosition.Sliding:
					{
						rectangle.X = sliderWidth - size.Width;
						val3.Alignment = (StringAlignment)1;
						SolidBrush val4 = new SolidBrush(((Control)this).Parent.BackColor);
						try
						{
							Rectangle rectangle2 = rectSlider;
							rectangle2.Y = rectangle.Y;
							rectangle2.Height = rectangle.Height;
							graph.FillRectangle((Brush)(object)val4, rectangle2);
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
						break;
					}
					}
					graph.FillRectangle((Brush)(object)val2, rectangle);
					graph.DrawString(text, ((Control)this).Font, (Brush)(object)val, (RectangleF)rectangle, val3);
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
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
