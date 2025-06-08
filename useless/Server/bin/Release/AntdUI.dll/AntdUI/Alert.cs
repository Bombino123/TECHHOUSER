using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

[Description("Alert 警告提示")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
[Designer(typeof(IControlDesigner))]
public class Alert : IControl, IEventListener
{
	private int radius = 6;

	private float borWidth;

	private string? text;

	private string? textTitle;

	private TType icon;

	private bool loop;

	private int loopSpeed = 10;

	private ITask? task;

	private int val;

	private Size? font_size;

	private readonly StringFormat stringLTEllipsis = Helper.SF_Ellipsis((StringAlignment)0, (StringAlignment)0);

	private readonly StringFormat stringCenter = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private readonly StringFormat stringLeft = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

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
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(0f)]
	public float BorderWidth
	{
		get
		{
			return borWidth;
		}
		set
		{
			if (borWidth != value)
			{
				borWidth = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

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
				if (loop && string.IsNullOrEmpty(value))
				{
					task?.Dispose();
					task = null;
				}
				font_size = null;
				text = value;
				((Control)this).Invalidate();
				((Control)this).OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("标题")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? TextTitle
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationTextTitle, textTitle);
		}
		set
		{
			if (!(textTitle == value))
			{
				textTitle = value;
				((Control)this).Invalidate();
				OnPropertyChanged("TextTitle");
			}
		}
	}

	[Description("标题")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTextTitle { get; set; }

	[Description("样式")]
	[Category("外观")]
	[DefaultValue(TType.None)]
	public TType Icon
	{
		get
		{
			return icon;
		}
		set
		{
			if (icon != value)
			{
				icon = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Icon");
			}
		}
	}

	[Description("文本轮播")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Loop
	{
		get
		{
			return loop;
		}
		set
		{
			if (loop != value)
			{
				loop = value;
				if (((Control)this).IsHandleCreated)
				{
					StartTask();
				}
				OnPropertyChanged("Loop");
			}
		}
	}

	[Description("文本轮播速率")]
	[Category("外观")]
	[DefaultValue(10)]
	public int LoopSpeed
	{
		get
		{
			return loopSpeed;
		}
		set
		{
			if (value < 1)
			{
				value = 1;
			}
			loopSpeed = value;
		}
	}

	public override Rectangle DisplayRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, borWidth / 2f * Config.Dpi);

	public override Rectangle ReadRectangle => ((Control)this).DisplayRectangle;

	public override GraphicsPath RenderRegion => ((Control)this).DisplayRectangle.RoundPath((float)radius * Config.Dpi);

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		((Control)(object)this).AddListener();
		if (loop)
		{
			StartTask();
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		font_size = null;
		((Control)this).OnFontChanged(e);
	}

	private void StartTask()
	{
		task?.Dispose();
		if (loop)
		{
			task = new ITask((Control)(object)this, delegate
			{
				//IL_007b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0080: Unknown result type (might be due to invalid IL or missing references)
				if (font_size.HasValue && font_size.Value.Width > 0)
				{
					val++;
					if (val > font_size.Value.Width)
					{
						if (((Control)this).Width > font_size.Value.Width)
						{
							val = 0;
						}
						else
						{
							int width = ((Control)this).Width;
							Padding padding = ((Control)this).Padding;
							val = -(width - ((Padding)(ref padding)).Horizontal);
						}
					}
					((Control)this).Invalidate();
				}
				else
				{
					Thread.Sleep(1000);
				}
				return loop;
			}, LoopSpeed);
		}
		else
		{
			((Control)this).Invalidate();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Expected O, but got Unknown
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Expected O, but got Unknown
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Expected O, but got Unknown
		Rectangle displayRectangle = ((Control)this).DisplayRectangle;
		Canvas canvas = e.Graphics.High();
		bool flag = string.IsNullOrEmpty(((Control)this).Text);
		if (icon == TType.None)
		{
			if (loop)
			{
				if (!font_size.HasValue && !flag)
				{
					font_size = canvas.MeasureString(((Control)this).Text, ((Control)this).Font);
				}
				if (font_size.HasValue)
				{
					canvas.SetClip(displayRectangle);
					PaintText(canvas, displayRectangle, font_size.Value, ((Control)this).ForeColor);
					canvas.ResetClip();
				}
			}
			else if (string.IsNullOrEmpty(TextTitle))
			{
				if (!flag)
				{
					Size value = canvas.MeasureString(((Control)this).Text, ((Control)this).Font);
					font_size = value;
					int num = (int)((float)(int)((float)value.Height * 0.86f) * 0.4f);
					canvas.String(rect: new Rectangle(displayRectangle.X + num, displayRectangle.Y, displayRectangle.Width - num * 2, displayRectangle.Height), text: ((Control)this).Text, font: ((Control)this).Font, color: ((Control)this).ForeColor, format: stringLeft);
				}
			}
			else
			{
				Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 1.14f, ((Control)this).Font.Style);
				try
				{
					Size size = canvas.MeasureString(TextTitle, val);
					int num2 = (int)((float)size.Height * 1.2f);
					int num3 = (int)((float)num2 * 0.5f);
					SolidBrush val2 = new SolidBrush(((Control)this).ForeColor);
					try
					{
						Rectangle rect2 = new Rectangle(displayRectangle.X + num3, displayRectangle.Y + num3, displayRectangle.Width - num3 * 2, size.Height);
						canvas.String(TextTitle, val, (Brush)(object)val2, rect2, stringLeft);
						int num4 = rect2.Bottom + (int)((float)num2 * 0.33f);
						canvas.String(rect: new Rectangle(rect2.X, num4, rect2.Width, displayRectangle.Height - (num4 + num3)), text: ((Control)this).Text, font: ((Control)this).Font, brush: (Brush)(object)val2, format: stringLTEllipsis);
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
		else
		{
			float num5 = (float)radius * Config.Dpi;
			Color color = Colour.Text.Get("Alert");
			Color color2;
			Color color3;
			switch (icon)
			{
			case TType.Success:
				color2 = Colour.SuccessBg.Get("Alert");
				color3 = Colour.SuccessBorder.Get("Alert");
				break;
			case TType.Info:
				color2 = Colour.InfoBg.Get("Alert");
				color3 = Colour.InfoBorder.Get("Alert");
				break;
			case TType.Warn:
				color2 = Colour.WarningBg.Get("Alert");
				color3 = Colour.WarningBorder.Get("Alert");
				break;
			case TType.Error:
				color2 = Colour.ErrorBg.Get("Alert");
				color3 = Colour.ErrorBorder.Get("Alert");
				break;
			default:
				color2 = Colour.SuccessBg.Get("Alert");
				color3 = Colour.SuccessBorder.Get("Alert");
				break;
			}
			GraphicsPath val3 = displayRectangle.RoundPath(num5);
			try
			{
				Size size2 = canvas.MeasureString("龍Qq", ((Control)this).Font);
				canvas.Fill(color2, val3);
				if (loop)
				{
					if (!font_size.HasValue && !flag)
					{
						font_size = canvas.MeasureString(((Control)this).Text, ((Control)this).Font);
					}
					if (font_size.HasValue)
					{
						int num6 = (int)((float)size2.Height * 0.86f);
						int x = (int)((float)num6 * 0.4f);
						Rectangle rectangle = new Rectangle(x, displayRectangle.Y + (displayRectangle.Height - num6) / 2, num6, num6);
						PaintText(canvas, displayRectangle, rectangle, font_size.Value, color, color2, num5);
						canvas.ResetClip();
						canvas.PaintIcons(icon, rectangle, Colour.BgBase.Get("Alert"), "Alert");
					}
				}
				else
				{
					if (!flag)
					{
						Size value2 = canvas.MeasureString(((Control)this).Text, ((Control)this).Font);
						font_size = value2;
					}
					if (string.IsNullOrEmpty(TextTitle))
					{
						int num7 = (int)((float)size2.Height * 0.86f);
						int num8 = (int)((float)num7 * 0.4f);
						Rectangle rect4 = new Rectangle(displayRectangle.X + num8, displayRectangle.Y + (displayRectangle.Height - num7) / 2, num7, num7);
						canvas.PaintIcons(icon, rect4, Colour.BgBase.Get("Alert"), "Alert");
						canvas.String(rect: new Rectangle(rect4.X + rect4.Width + num8, displayRectangle.Y, displayRectangle.Width - (rect4.Width + num8 * 3), displayRectangle.Height), text: ((Control)this).Text, font: ((Control)this).Font, color: color, format: stringLeft);
					}
					else
					{
						Font val4 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 1.14f, ((Control)this).Font.Style);
						try
						{
							int num9 = (int)((float)size2.Height * 1.2f);
							int num10 = (int)((float)num9 * 0.5f);
							Rectangle rect6 = new Rectangle(displayRectangle.X + num10, displayRectangle.Y + num10, num9, num9);
							canvas.PaintIcons(icon, rect6, Colour.BgBase.Get("Alert"), "Alert");
							SolidBrush val5 = new SolidBrush(color);
							try
							{
								Rectangle rect7 = new Rectangle(rect6.X + rect6.Width + num9 / 2, rect6.Y, displayRectangle.Width - (rect6.Width + num10 * 3), rect6.Height);
								canvas.String(TextTitle, val4, (Brush)(object)val5, rect7, stringLeft);
								int num11 = rect7.Bottom + (int)((float)num9 * 0.2f);
								canvas.String(rect: new Rectangle(rect7.X, num11, rect7.Width, displayRectangle.Height - (num11 + num10)), text: ((Control)this).Text, font: ((Control)this).Font, brush: (Brush)(object)val5, format: stringLTEllipsis);
							}
							finally
							{
								((IDisposable)val5)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
					}
				}
				if (borWidth > 0f)
				{
					canvas.Draw(color3, borWidth * Config.Dpi, val3);
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintText(Canvas g, Rectangle rect, Size size, Color fore)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(fore);
		try
		{
			if (string.IsNullOrEmpty(TextTitle))
			{
				Rectangle rect2 = new Rectangle(rect.X - this.val, rect.Y, size.Width, rect.Height);
				g.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val, rect2, stringCenter);
				if (rect.Width > size.Width)
				{
					int num = rect.Width + rect2.Width / 2;
					Rectangle rect3 = new Rectangle(rect2.Right, rect2.Y, rect2.Width, rect2.Height);
					while (rect3.X < num)
					{
						g.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val, rect3, stringCenter);
						rect3.X = rect3.Right;
					}
				}
				return;
			}
			Size size2 = g.MeasureString(TextTitle, ((Control)this).Font);
			Rectangle rect4 = new Rectangle(rect.X + size2.Width - this.val, rect.Y, size.Width, rect.Height);
			g.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val, rect4, stringCenter);
			if (rect.Width > size.Width)
			{
				int num2 = rect.Width + rect4.Width / 2;
				Rectangle rect5 = new Rectangle(rect4.Right, rect4.Y, rect4.Width, rect4.Height);
				while (rect5.X < num2)
				{
					g.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val, rect5, stringCenter);
					rect5.X = rect5.Right;
				}
			}
			Rectangle rectangle = new Rectangle(rect.X, rect.Y, (size.Height + size2.Width) * 2, rect.Height);
			LinearGradientBrush val2 = new LinearGradientBrush(rectangle, ((Control)this).BackColor, Color.Transparent, 0f);
			try
			{
				g.Fill((Brush)(object)val2, rectangle);
				g.Fill((Brush)(object)val2, rectangle);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			g.String(TextTitle, ((Control)this).Font, (Brush)(object)val, new Rectangle(rect.X, rect.Y, size2.Width, rect.Height), stringCenter);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintText(Canvas g, Rectangle rect, Rectangle rect_icon, Size size, Color fore, Color back, float radius)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(fore);
		try
		{
			Rectangle rect2 = new Rectangle(rect.X - this.val, rect.Y, size.Width, rect.Height);
			g.SetClip(new Rectangle(rect.X, rect2.Y + (rect.Height - size.Height) / 2, rect.Width, size.Height));
			g.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val, rect2, stringCenter);
			if (rect.Width > size.Width)
			{
				int num = rect.Width + rect2.Width / 2;
				Rectangle rect3 = new Rectangle(rect2.Right, rect2.Y, rect2.Width, rect2.Height);
				while (rect3.X < num)
				{
					g.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val, rect3, stringCenter);
					rect3.X = rect3.Right;
				}
			}
			Rectangle rectangle;
			if (string.IsNullOrEmpty(TextTitle))
			{
				rectangle = new Rectangle(rect.X, rect.Y, size.Height * 2, rect.Height);
				LinearGradientBrush val2 = new LinearGradientBrush(rectangle, back, Color.Transparent, 0f);
				try
				{
					rectangle.Width--;
					g.Fill((Brush)(object)val2, rectangle);
					g.Fill((Brush)(object)val2, rectangle);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			else
			{
				Size size2 = g.MeasureString(TextTitle, ((Control)this).Font);
				rectangle = new Rectangle(rect.X, rect.Y, (size.Height + size2.Width) * 2, rect.Height);
				LinearGradientBrush val3 = new LinearGradientBrush(rectangle, back, Color.Transparent, 0f);
				try
				{
					g.Fill((Brush)(object)val3, rectangle);
					g.Fill((Brush)(object)val3, rectangle);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				g.String(TextTitle, ((Control)this).Font, (Brush)(object)val, new Rectangle(rect_icon.Right, rect.Y, size2.Width, rect.Height), stringCenter);
			}
			Rectangle rectangle2 = new Rectangle(rect.Right - rectangle.Width, rectangle.Y, rectangle.Width, rectangle.Height);
			LinearGradientBrush val4 = new LinearGradientBrush(rectangle2, Color.Transparent, back, 0f);
			try
			{
				rectangle2.X++;
				rectangle2.Width--;
				g.Fill((Brush)(object)val4, rectangle2);
				g.Fill((Brush)(object)val4, rectangle2);
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		task?.Dispose();
		((Control)this).OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		if (loop)
		{
			StartTask();
		}
		((Control)this).OnMouseLeave(e);
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			font_size = null;
		}
	}
}
