using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Panel 面板")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
[Designer(typeof(IControlDesigner))]
public class Panel : IControl, ShadowConfig, IMessageFilter, IEventListener
{
	private int radius = 6;

	private Padding _padding = new Padding(0);

	private int shadow;

	private Color? shadowColor;

	private int shadowOffsetX;

	private int shadowOffsetY;

	private float shadowOpacity = 0.1f;

	private float shadowOpacityHover = 0.3f;

	private TAlignMini shadowAlign;

	private Color? back;

	private Image? backImage;

	private TFit backFit;

	private int arrwoSize = 8;

	private TAlign arrowAlign;

	private float borderWidth;

	private Color? borderColor;

	private DashStyle borderStyle;

	private Bitmap? shadow_temp;

	private float AnimationHoverValue = 0.1f;

	private bool AnimationHover;

	private bool _mouseHover;

	private ITask? ThreadHover;

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

	[Description("内边距")]
	[Category("外观")]
	[DefaultValue(typeof(Padding), "0, 0, 0, 0")]
	public Padding padding
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _padding;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			if (!(_padding == value))
			{
				_padding = value;
				Bitmap? obj = shadow_temp;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				shadow_temp = null;
				IOnSizeChanged();
				OnPropertyChanged("padding");
			}
		}
	}

	[Description("阴影")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (shadow != value)
			{
				shadow = value;
				Bitmap? obj = shadow_temp;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				shadow_temp = null;
				IOnSizeChanged();
				OnPropertyChanged("Shadow");
			}
		}
	}

	[Description("阴影颜色")]
	[Category("阴影")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			if (!(shadowColor == value))
			{
				shadowColor = value;
				Bitmap? obj = shadow_temp;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				shadow_temp = null;
				((Control)this).Invalidate();
				OnPropertyChanged("ShadowColor");
			}
		}
	}

	[Description("阴影偏移X")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetX
	{
		get
		{
			return shadowOffsetX;
		}
		set
		{
			if (shadowOffsetX != value)
			{
				shadowOffsetX = value;
				Bitmap? obj = shadow_temp;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				shadow_temp = null;
				IOnSizeChanged();
				OnPropertyChanged("ShadowOffsetX");
			}
		}
	}

	[Description("阴影偏移Y")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetY
	{
		get
		{
			return shadowOffsetY;
		}
		set
		{
			if (shadowOffsetY != value)
			{
				shadowOffsetY = value;
				Bitmap? obj = shadow_temp;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				shadow_temp = null;
				IOnSizeChanged();
				OnPropertyChanged("ShadowOffsetY");
			}
		}
	}

	[Description("阴影透明度")]
	[Category("阴影")]
	[DefaultValue(0.1f)]
	public float ShadowOpacity
	{
		get
		{
			return shadowOpacity;
		}
		set
		{
			if (shadowOpacity != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				shadowOpacity = value;
				AnimationHoverValue = shadowOpacity;
				((Control)this).Invalidate();
				OnPropertyChanged("ShadowOpacity");
			}
		}
	}

	[Description("阴影透明度动画使能")]
	[Category("阴影")]
	[DefaultValue(false)]
	public bool ShadowOpacityAnimation { get; set; }

	[Description("悬停阴影后透明度")]
	[Category("阴影")]
	[DefaultValue(0.3f)]
	public float ShadowOpacityHover
	{
		get
		{
			return shadowOpacityHover;
		}
		set
		{
			if (shadowOpacityHover != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				shadowOpacityHover = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ShadowOpacityHover");
			}
		}
	}

	[Description("阴影方向")]
	[Category("阴影")]
	[DefaultValue(TAlignMini.None)]
	public TAlignMini ShadowAlign
	{
		get
		{
			return shadowAlign;
		}
		set
		{
			if (shadowAlign != value)
			{
				shadowAlign = value;
				Bitmap? obj = shadow_temp;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				shadow_temp = null;
				IOnSizeChanged();
				OnPropertyChanged("ShadowAlign");
			}
		}
	}

	[Description("背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Back
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
				OnPropertyChanged("Back");
			}
		}
	}

	[Description("背景图片")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? BackgroundImage
	{
		get
		{
			return backImage;
		}
		set
		{
			if (backImage != value)
			{
				backImage = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackgroundImage");
			}
		}
	}

	[Description("背景图片布局")]
	[Category("外观")]
	[DefaultValue(TFit.Fill)]
	public TFit BackgroundImageLayout
	{
		get
		{
			return backFit;
		}
		set
		{
			if (backFit != value)
			{
				backFit = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackgroundImageLayout");
			}
		}
	}

	[Description("箭头大小")]
	[Category("箭头")]
	[DefaultValue(8)]
	public int ArrowSize
	{
		get
		{
			return arrwoSize;
		}
		set
		{
			if (arrwoSize != value)
			{
				arrwoSize = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ArrowSize");
			}
		}
	}

	[Description("箭头方向")]
	[Category("箭头")]
	[DefaultValue(TAlign.None)]
	public TAlign ArrowAlign
	{
		get
		{
			return arrowAlign;
		}
		set
		{
			if (arrowAlign != value)
			{
				arrowAlign = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ArrowAlign");
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
			return borderWidth;
		}
		set
		{
			if (borderWidth != value)
			{
				borderWidth = value;
				IOnSizeChanged();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

	[Description("边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			if (!(borderColor == value))
			{
				borderColor = value;
				if (borderWidth > 0f)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("BorderColor");
			}
		}
	}

	[Description("边框样式")]
	[Category("边框")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public DashStyle BorderStyle
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return borderStyle;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (borderStyle != value)
			{
				borderStyle = value;
				if (borderWidth > 0f)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("BorderStyle");
			}
		}
	}

	public override Rectangle DisplayRectangle => ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding, this, shadowAlign, borderWidth);

	public override Rectangle ReadRectangle => ((Control)this).ClientRectangle.DeflateRect(_padding).PaddingRect(this, shadowAlign, borderWidth / 2f);

	public override GraphicsPath RenderRegion => ReadRectangle.RoundPath((float)radius * Config.Dpi);

	private bool ExtraMouseHover
	{
		get
		{
			return _mouseHover;
		}
		set
		{
			if (_mouseHover == value)
			{
				return;
			}
			_mouseHover = value;
			if (!base.Enabled || !ShadowOpacityAnimation || shadow <= 0 || !(shadowOpacityHover > 0f) || !(shadowOpacityHover > shadowOpacity))
			{
				return;
			}
			if (Config.Animation)
			{
				ThreadHover?.Dispose();
				AnimationHover = true;
				float addvalue = shadowOpacityHover / 12f;
				if (value)
				{
					ThreadHover = new ITask((Control)(object)this, delegate
					{
						AnimationHoverValue = AnimationHoverValue.Calculate(addvalue);
						if (AnimationHoverValue >= shadowOpacityHover)
						{
							AnimationHoverValue = shadowOpacityHover;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 20, delegate
					{
						AnimationHover = false;
						((Control)this).Invalidate();
					});
					return;
				}
				ThreadHover = new ITask((Control)(object)this, delegate
				{
					AnimationHoverValue = AnimationHoverValue.Calculate(0f - addvalue);
					if (AnimationHoverValue <= shadowOpacity)
					{
						AnimationHoverValue = shadowOpacity;
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 20, delegate
				{
					AnimationHover = false;
					((Control)this).Invalidate();
				});
			}
			else
			{
				((Control)this).Invalidate();
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width <= 0 || clientRectangle.Height <= 0)
		{
			return;
		}
		Canvas canvas = e.Graphics.High();
		Rectangle readRectangle = ReadRectangle;
		float num = (float)radius * Config.Dpi;
		SolidBrush val = new SolidBrush(back ?? Colour.BgContainer.Get("Panel"));
		try
		{
			GraphicsPath val2 = DrawShadow(canvas, num, clientRectangle, readRectangle);
			try
			{
				canvas.Fill((Brush)(object)val, val2);
				if (backImage != null)
				{
					canvas.Image(readRectangle, backImage, backFit, num, round: false);
				}
				if (borderWidth > 0f)
				{
					canvas.Draw(borderColor ?? Colour.BorderColor.Get("Panel"), borderWidth * Config.Dpi, borderStyle, val2);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			if (ArrowAlign != 0)
			{
				canvas.FillPolygon((Brush)(object)val, ArrowAlign.AlignLines(ArrowSize, clientRectangle, readRectangle));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private GraphicsPath DrawShadow(Canvas g, float radius, Rectangle rect_client, Rectangle rect_read)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		GraphicsPath val = rect_read.RoundPath(radius, shadowAlign);
		if (shadow > 0)
		{
			int range = (int)((float)Shadow * Config.Dpi);
			int num = (int)((float)ShadowOffsetX * Config.Dpi);
			int num2 = (int)((float)ShadowOffsetY * Config.Dpi);
			if (shadow_temp == null || ((Image)shadow_temp).Width != rect_client.Width || ((Image)shadow_temp).Height != rect_client.Height)
			{
				Bitmap? obj = shadow_temp;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				shadow_temp = val.PaintShadow(rect_client.Width, rect_client.Height, shadowColor ?? Colour.TextBase.Get("Panel"), range);
			}
			ImageAttributes val2 = new ImageAttributes();
			try
			{
				ColorMatrix val3 = new ColorMatrix();
				if (AnimationHover)
				{
					val3.Matrix33 = AnimationHoverValue;
				}
				else if (ExtraMouseHover)
				{
					val3.Matrix33 = shadowOpacityHover;
				}
				else
				{
					val3.Matrix33 = shadowOpacity;
				}
				val2.SetColorMatrix(val3, (ColorMatrixFlag)0, (ColorAdjustType)1);
				g.Image((Image)(object)shadow_temp, new Rectangle(rect_client.X + num, rect_client.Y + num2, rect_client.Width, rect_client.Height), 0, 0, ((Image)shadow_temp).Width, ((Image)shadow_temp).Height, (GraphicsUnit)2, val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		return val;
	}

	public void SetMouseHover(bool val)
	{
		ExtraMouseHover = val;
	}

	protected override void Dispose(bool disposing)
	{
		ThreadHover?.Dispose();
		ThreadHover = null;
		Application.RemoveMessageFilter((IMessageFilter)(object)this);
		Bitmap? obj = shadow_temp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		shadow_temp = null;
		base.Dispose(disposing);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
		ExtraMouseHover = true;
	}

	public bool PreFilterMessage(ref Message m)
	{
		if (((Message)(ref m)).Msg == 673 || ((Message)(ref m)).Msg == 675)
		{
			ExtraMouseHover = ((Control)this).ClientRectangle.Contains(((Control)this).PointToClient(Control.MousePosition));
			return false;
		}
		return false;
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		Application.AddMessageFilter((IMessageFilter)(object)this);
		((Control)(object)this).AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.THEME)
		{
			Bitmap? obj = shadow_temp;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			shadow_temp = null;
		}
	}
}
