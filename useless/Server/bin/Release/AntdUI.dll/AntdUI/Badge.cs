using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Badge 徽标数")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
public class Badge : IControl, IEventListener
{
	private Color? fore;

	private TState state = TState.Default;

	private float dotratio = 0.4f;

	private int gap;

	private bool has_text = true;

	private string? text;

	private StringFormat s_f = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private ContentAlignment textAlign = (ContentAlignment)16;

	private Color? fill;

	private ITask? ThreadState;

	private float AnimationStateValue;

	private TAutoSize autoSize;

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

	[Description("状态")]
	[Category("外观")]
	[DefaultValue(TState.Default)]
	public TState State
	{
		get
		{
			return state;
		}
		set
		{
			if (state != value)
			{
				state = value;
				StartAnimation();
				((Control)this).Invalidate();
				OnPropertyChanged("State");
			}
		}
	}

	[Description("点比例")]
	[Category("外观")]
	[DefaultValue(0.4f)]
	public float DotRatio
	{
		get
		{
			return dotratio;
		}
		set
		{
			if (dotratio != value)
			{
				dotratio = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("DotRatio");
			}
		}
	}

	[Description("间隔")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Gap
	{
		get
		{
			return gap;
		}
		set
		{
			if (gap != value)
			{
				gap = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Gap");
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
				text = value;
				has_text = string.IsNullOrEmpty(text);
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				((Control)this).OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("文本位置")]
	[Category("外观")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public ContentAlignment TextAlign
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return textAlign;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (textAlign != value)
			{
				textAlign = value;
				textAlign.SetAlignment(ref s_f);
				((Control)this).Invalidate();
				OnPropertyChanged("TextAlign");
			}
		}
	}

	[Description("颜色")]
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
				OnPropertyChanged("Fill");
			}
		}
	}

	[Browsable(true)]
	[Description("自动大小")]
	[Category("外观")]
	[DefaultValue(false)]
	public override bool AutoSize
	{
		get
		{
			return ((Control)this).AutoSize;
		}
		set
		{
			if (((Control)this).AutoSize == value)
			{
				return;
			}
			((Control)this).AutoSize = value;
			if (value)
			{
				if (autoSize == TAutoSize.None)
				{
					autoSize = TAutoSize.Auto;
				}
			}
			else
			{
				autoSize = TAutoSize.None;
			}
			BeforeAutoSize();
		}
	}

	[Description("自动大小模式")]
	[Category("外观")]
	[DefaultValue(TAutoSize.None)]
	public TAutoSize AutoSizeMode
	{
		get
		{
			return autoSize;
		}
		set
		{
			if (autoSize != value)
			{
				autoSize = value;
				((Control)this).AutoSize = autoSize != TAutoSize.None;
				BeforeAutoSize();
			}
		}
	}

	private Size PSize => Helper.GDI(delegate(Canvas g)
	{
		if (has_text)
		{
			Size result = g.MeasureString("龍Qq", ((Control)this).Font);
			result.Width = result.Height;
			return result;
		}
		Size result2 = g.MeasureString(((Control)this).Text ?? "龍Qq", ((Control)this).Font);
		result2.Width += result2.Height;
		return result2;
	});

	private void StartAnimation()
	{
		StopAnimation();
		if (Config.Animation && state == TState.Processing)
		{
			ThreadState = new ITask((Control)(object)this, delegate(float i)
			{
				AnimationStateValue = i;
				((Control)this).Invalidate();
			}, 50, 1f, 0.05f);
		}
	}

	private void StopAnimation()
	{
		ThreadState?.Dispose();
	}

	protected override void Dispose(bool disposing)
	{
		StopAnimation();
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		Rectangle rectangle = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		Canvas canvas = e.Graphics.High();
		if (has_text)
		{
			Size size = canvas.MeasureString("龍Qq", ((Control)this).Font);
			int num = (int)((float)size.Height * dotratio);
			SolidBrush val = new SolidBrush(GetColor(fill, state));
			try
			{
				canvas.FillEllipse((Brush)(object)val, new RectangleF((float)(rectangle.Width - num) / 2f, (float)(rectangle.Height - num) / 2f, num, num));
				if (state == TState.Processing)
				{
					float num2 = (float)size.Height * AnimationStateValue;
					float alpha = 255f * (1f - AnimationStateValue);
					canvas.DrawEllipse(Helper.ToColor(alpha, val.Color), 4f * Config.Dpi, new RectangleF(((float)rectangle.Width - num2) / 2f, ((float)rectangle.Height - num2) / 2f, num2, num2));
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			Size size2 = canvas.MeasureString(((Control)this).Text, ((Control)this).Font);
			int num3 = (int)((float)size2.Height * dotratio);
			int num4 = (int)((float)gap * Config.Dpi);
			SolidBrush val2 = new SolidBrush(GetColor(fill, state));
			try
			{
				RectangleF rect = new RectangleF(rectangle.X + (size2.Height - num3) / 2, rectangle.Y + (rectangle.Height - num3) / 2, num3, num3);
				canvas.FillEllipse((Brush)(object)val2, rect);
				if (state == TState.Processing)
				{
					float num5 = (float)size2.Height * AnimationStateValue;
					float alpha2 = 255f * (1f - AnimationStateValue);
					canvas.DrawEllipse(Helper.ToColor(alpha2, val2.Color), 4f * Config.Dpi, new RectangleF(rect.X + (rect.Width - num5) / 2f, rect.Y + (rect.Height - num5) / 2f, num5, num5));
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			SolidBrush val3 = fore.Brush(Colour.Text.Get("Badge"), Colour.TextQuaternary.Get("Badge"), base.Enabled);
			try
			{
				canvas.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val3, new Rectangle(rectangle.X + num4 + size2.Height, rectangle.Y, rectangle.Width - size2.Height, rectangle.Height), s_f);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private Color GetColor(Color? color, TState state)
	{
		if (color.HasValue)
		{
			return color.Value;
		}
		return GetColor(state);
	}

	private Color GetColor(TState state)
	{
		switch (state)
		{
		case TState.Success:
			return Colour.Success.Get("Badge");
		case TState.Error:
			return Colour.Error.Get("Badge");
		case TState.Primary:
		case TState.Processing:
			return Colour.Primary.Get("Badge");
		case TState.Warn:
			return Colour.Warning.Get("Badge");
		default:
			return Colour.TextQuaternary.Get("Badge");
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		BeforeAutoSize();
		((Control)this).OnFontChanged(e);
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		if (autoSize == TAutoSize.None)
		{
			return ((Control)this).GetPreferredSize(proposedSize);
		}
		if (autoSize == TAutoSize.Width)
		{
			return new Size(PSize.Width, ((Control)this).GetPreferredSize(proposedSize).Height);
		}
		if (autoSize == TAutoSize.Height)
		{
			return new Size(((Control)this).GetPreferredSize(proposedSize).Width, PSize.Height);
		}
		return PSize;
	}

	protected override void OnResize(EventArgs e)
	{
		BeforeAutoSize();
		((Control)this).OnResize(e);
	}

	private bool BeforeAutoSize()
	{
		if (autoSize == TAutoSize.None)
		{
			return true;
		}
		if (((Control)this).InvokeRequired)
		{
			bool flag = false;
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				flag = BeforeAutoSize();
			});
			return flag;
		}
		Size pSize = PSize;
		switch (autoSize)
		{
		case TAutoSize.Width:
			if (((Control)this).Width == pSize.Width)
			{
				return true;
			}
			((Control)this).Width = pSize.Width;
			break;
		case TAutoSize.Height:
			if (((Control)this).Height == pSize.Height)
			{
				return true;
			}
			((Control)this).Height = pSize.Height;
			break;
		default:
			if (((Control)this).Width == pSize.Width && ((Control)this).Height == pSize.Height)
			{
				return true;
			}
			((Control)this).Size = pSize;
			break;
		}
		return false;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		((Control)(object)this).AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			BeforeAutoSize();
		}
	}
}
