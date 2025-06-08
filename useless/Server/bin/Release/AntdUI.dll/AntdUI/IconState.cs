using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Icon 状态图标")]
[ToolboxItem(true)]
public class IconState : IControl
{
	private Color? back;

	private Color? color;

	private TType state = TType.Success;

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

	[Description("颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Color
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
				OnPropertyChanged("Color");
			}
		}
	}

	[Description("状态")]
	[Category("外观")]
	[DefaultValue(TType.Success)]
	public TType State
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
				((Control)this).Invalidate();
				OnPropertyChanged("State");
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		Canvas canvas = e.Graphics.High();
		if (state == TType.None)
		{
			this.PaintBadge(canvas);
		}
		else
		{
			Rectangle rect = ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding);
			int num = ((rect.Width > rect.Height) ? rect.Height : rect.Width);
			Rectangle rectangle = new Rectangle((rect.Width - num) / 2, (rect.Height - num) / 2, num, num);
			if (color.HasValue)
			{
				SolidBrush val = new SolidBrush(color.Value);
				try
				{
					canvas.FillEllipse((Brush)(object)val, new RectangleF(rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2));
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			switch (state)
			{
			case TType.Success:
				canvas.GetImgExtend(SvgDb.IcoSuccess, rect, back ?? Colour.Success.Get("IconComplete"));
				break;
			case TType.Error:
				canvas.GetImgExtend(SvgDb.IcoError, rect, back ?? Colour.Error.Get("IconError"));
				break;
			case TType.Info:
				canvas.GetImgExtend(SvgDb.IcoInfo, rect, back ?? Colour.Info.Get("IconInfo"));
				break;
			case TType.Warn:
				canvas.GetImgExtend(SvgDb.IcoWarn, rect, back ?? Colour.Warning.Get("IconWarn"));
				break;
			}
			this.PaintBadge(canvas);
		}
		((Control)this).OnPaint(e);
	}
}
