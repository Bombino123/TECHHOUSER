using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class SpinForm : ILayeredFormOpacity
{
	private Control control;

	private Form? parent;

	private Spin.Config config;

	private GraphicsPath? gpath;

	private int Radius;

	private int Bor;

	private bool HasBor;

	private SpinCore spin_core = new SpinCore();

	public SpinForm(Control _control, Form? _parent, Spin.Config _config)
	{
		maxalpha = byte.MaxValue;
		this.control = _control;
		parent = _parent;
		((Control)this).Font = _control.Font;
		config = _config;
		_control.SetTopMost(((Control)this).Handle);
		SetSize(_control.Size);
		SetLocation(_control.PointToScreen(Point.Empty));
		if (_config.Radius.HasValue)
		{
			Radius = _config.Radius.Value;
			return;
		}
		if (_control is IControl control)
		{
			gpath = control.RenderRegion;
			return;
		}
		Form val = (Form)(object)((_control is Form) ? _control : null);
		if (val != null)
		{
			HasBor = val.FormFrame(out Radius, out Bor);
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		spin_core.Start(this);
		if (parent != null)
		{
			((Control)parent).LocationChanged += Parent_LocationChanged;
			((Control)parent).SizeChanged += Parent_SizeChanged;
		}
	}

	private void Parent_LocationChanged(object? sender, EventArgs e)
	{
		SetLocation(control.PointToScreen(Point.Empty));
	}

	private void Parent_SizeChanged(object? sender, EventArgs e)
	{
		SetLocation(this.control.PointToScreen(Point.Empty));
		SetSize(this.control.Size);
		if (!config.Radius.HasValue && this.control is IControl control)
		{
			GraphicsPath? obj = gpath;
			if (obj != null)
			{
				obj.Dispose();
			}
			gpath = control.RenderRegion;
		}
	}

	public override Bitmap PrintBit()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rect = (HasBor ? new Rectangle(Bor, 0, targetRectXY.Width - Bor * 2, targetRectXY.Height - Bor) : targetRectXY);
		Bitmap val = new Bitmap(rect.Width, rect.Height);
		using Canvas canvas = Graphics.FromImage((Image)(object)val).HighLay(text: true);
		SolidBrush val2 = new SolidBrush(config.Back ?? Colour.BgBase.Get("Spin").rgba(0.8f));
		try
		{
			if (gpath != null)
			{
				canvas.Fill((Brush)(object)val2, gpath);
			}
			else if (Radius > 0)
			{
				GraphicsPath val3 = rect.RoundPath(Radius);
				try
				{
					canvas.Fill((Brush)(object)val2, val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			else
			{
				canvas.Fill((Brush)(object)val2, rect);
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		spin_core.Paint(canvas, rect, config, (Control)(object)this);
		return val;
	}

	protected override void Dispose(bool disposing)
	{
		spin_core.Dispose();
		if (parent != null)
		{
			((Control)parent).LocationChanged -= Parent_LocationChanged;
			((Control)parent).SizeChanged -= Parent_SizeChanged;
		}
		GraphicsPath? obj = gpath;
		if (obj != null)
		{
			obj.Dispose();
		}
		base.Dispose(disposing);
		_ = control;
	}
}
