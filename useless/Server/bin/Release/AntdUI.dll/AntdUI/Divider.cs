using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Divider 分割线")]
[ToolboxItem(true)]
[Designer(typeof(IControlDesigner))]
public class Divider : IControl
{
	private TOrientation orientation;

	private float orientationMargin = 0.02f;

	private float textPadding = 0.4f;

	private float thickness = 0.6f;

	private Color? color;

	private string? text;

	private readonly StringFormat s_f_all = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	private readonly StringFormat s_f = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)1);

	[Description("是否竖向")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Vertical { get; set; }

	[Description("方向")]
	[Category("外观")]
	[DefaultValue(TOrientation.None)]
	public TOrientation Orientation
	{
		get
		{
			return orientation;
		}
		set
		{
			if (orientation != value)
			{
				orientation = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("文本与边缘距离，取值 0 ～ 1")]
	[Category("外观")]
	[DefaultValue(0.02f)]
	public float OrientationMargin
	{
		get
		{
			return orientationMargin;
		}
		set
		{
			if (orientationMargin != value)
			{
				orientationMargin = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("文本与线距离，同等字体大小")]
	[Category("外观")]
	[DefaultValue(0.4f)]
	public float TextPadding
	{
		get
		{
			return textPadding;
		}
		set
		{
			if (textPadding != value)
			{
				textPadding = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("厚度")]
	[Category("外观")]
	[DefaultValue(0.6f)]
	public float Thickness
	{
		get
		{
			return thickness;
		}
		set
		{
			if (thickness != value)
			{
				thickness = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Thickness");
			}
		}
	}

	[Description("线颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ColorSplit
	{
		get
		{
			return color;
		}
		set
		{
			if (!(color == value))
			{
				color = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ColorSplit");
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

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		Rectangle rectangle = clientRectangle.PaddingRect(((Control)this).Margin);
		if (rectangle.Width == 0 || rectangle.Height == 0)
		{
			return;
		}
		Canvas canvas = e.Graphics.High();
		SolidBrush val = color.Brush(Colour.Split.Get("Divider"));
		try
		{
			if (((Control)this).Text != null)
			{
				bool enabled = base.Enabled;
				if (Vertical)
				{
					string text = string.Join(Environment.NewLine, ((Control)this).Text.ToCharArray());
					Size size = canvas.MeasureString(text, ((Control)this).Font, 0, s_f_all);
					int num = (int)((float)rectangle.Height * orientationMargin);
					int num2 = (int)((float)size.Width * TextPadding);
					float x = (float)rectangle.X + ((float)rectangle.Width - thickness) / 2f;
					switch (Orientation)
					{
					case TOrientation.Left:
						if (num > 0)
						{
							Rectangle rect3 = new Rectangle(rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Y + num + num2, size.Width, size.Height);
							canvas.Fill((Brush)(object)val, new RectangleF(x, rectangle.Y, thickness, num));
							canvas.Fill((Brush)(object)val, new RectangleF(x, rect3.Bottom + num2, thickness, (float)(rectangle.Height - size.Height - num) - (float)num2 * 2f));
							canvas.String(text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), rect3, s_f);
						}
						else
						{
							Rectangle rect4 = new Rectangle(rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Y, size.Width, size.Height);
							canvas.Fill((Brush)(object)val, new RectangleF(x, rect4.Bottom + num2, thickness, rectangle.Height - size.Height - num2));
							canvas.String(text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), rect4, s_f);
						}
						break;
					case TOrientation.Right:
						if (num > 0)
						{
							Rectangle rect = new Rectangle(rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Bottom - size.Height - num - num2, size.Width, size.Height);
							canvas.Fill((Brush)(object)val, new RectangleF(x, rectangle.Y, thickness, (float)(rectangle.Height - size.Height - num) - (float)num2 * 2f));
							canvas.Fill((Brush)(object)val, new RectangleF(x, rect.Bottom + num2, thickness, num));
							canvas.String(text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), rect, s_f);
						}
						else
						{
							Rectangle rect2 = new Rectangle(rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Bottom - size.Height, size.Width, size.Height);
							canvas.Fill((Brush)(object)val, new RectangleF(x, rectangle.Y, thickness, rectangle.Height - size.Height - num2));
							canvas.String(text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), rect2, s_f);
						}
						break;
					default:
					{
						float num3 = (rectangle.Height - size.Height) / 2 - num - num2;
						canvas.Fill((Brush)(object)val, new RectangleF(x, rectangle.Y, thickness, num3));
						canvas.Fill((Brush)(object)val, new RectangleF(x, (float)rectangle.Y + num3 + (float)size.Height + (float)(num + num2) * 2f, thickness, num3));
						canvas.String(text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), clientRectangle, s_f);
						break;
					}
					}
				}
				else
				{
					Size size2 = canvas.MeasureString(((Control)this).Text, ((Control)this).Font);
					int num4 = (int)((float)rectangle.Width * orientationMargin);
					int num5 = (int)((float)size2.Height * TextPadding);
					float y = (float)rectangle.Y + ((float)rectangle.Height - thickness) / 2f;
					switch (Orientation)
					{
					case TOrientation.Left:
						if (num4 > 0)
						{
							Rectangle rect7 = new Rectangle(rectangle.X + num4 + num5, rectangle.Y + (rectangle.Height - size2.Height) / 2, size2.Width, size2.Height);
							canvas.Fill((Brush)(object)val, new RectangleF(rectangle.X, y, num4, thickness));
							canvas.Fill((Brush)(object)val, new RectangleF(rect7.Right + num5, y, (float)(rectangle.Width - size2.Width - num4) - (float)num5 * 2f, thickness));
							canvas.String(((Control)this).Text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), rect7, s_f_all);
						}
						else
						{
							Rectangle rect8 = new Rectangle(rectangle.X, rectangle.Y + (rectangle.Height - size2.Height) / 2, size2.Width, size2.Height);
							canvas.Fill((Brush)(object)val, new RectangleF(rect8.Right + num5, y, rectangle.Width - size2.Width - num5, thickness));
							canvas.String(((Control)this).Text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), rect8, s_f_all);
						}
						break;
					case TOrientation.Right:
						if (num4 > 0)
						{
							Rectangle rect5 = new Rectangle(rectangle.Right - size2.Width - num4 - num5, rectangle.Y + (rectangle.Height - size2.Height) / 2, size2.Width, size2.Height);
							canvas.Fill((Brush)(object)val, new RectangleF(rectangle.X, y, (float)(rectangle.Width - size2.Width - num4) - (float)num5 * 2f, thickness));
							canvas.Fill((Brush)(object)val, new RectangleF(rect5.Right + num5, y, num4, thickness));
							canvas.String(((Control)this).Text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), rect5, s_f_all);
						}
						else
						{
							Rectangle rect6 = new Rectangle(rectangle.Right - size2.Width, rectangle.Y + (rectangle.Height - size2.Height) / 2, size2.Width, size2.Height);
							canvas.Fill((Brush)(object)val, new RectangleF(rectangle.X, y, rectangle.Width - size2.Width - num5, thickness));
							canvas.String(((Control)this).Text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), rect6, s_f_all);
						}
						break;
					default:
					{
						float num6 = (rectangle.Width - size2.Width) / 2 - num4 - num5;
						canvas.Fill((Brush)(object)val, new RectangleF(rectangle.X, y, num6, thickness));
						canvas.Fill((Brush)(object)val, new RectangleF((float)rectangle.X + num6 + (float)size2.Width + (float)(num4 + num5) * 2f, y, num6, thickness));
						canvas.String(((Control)this).Text, ((Control)this).Font, enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Divider"), clientRectangle, s_f_all);
						break;
					}
					}
				}
			}
			else if (Vertical)
			{
				canvas.Fill((Brush)(object)val, new RectangleF((float)rectangle.X + ((float)rectangle.Width - thickness) / 2f, rectangle.Y, thickness, rectangle.Height));
			}
			else
			{
				canvas.Fill((Brush)(object)val, new RectangleF(rectangle.X, (float)rectangle.Y + ((float)rectangle.Height - thickness) / 2f, rectangle.Width, thickness));
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
