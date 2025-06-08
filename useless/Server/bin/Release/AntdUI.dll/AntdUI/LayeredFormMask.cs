using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormMask : ILayeredFormOpacity
{
	private int Radius;

	private int Bor;

	private bool HasBor;

	private Form form;

	private Bitmap? temp;

	public LayeredFormMask(Form _form)
	{
		form = _form;
		((Form)this).TopMost = _form.TopMost;
		HasBor = form.FormFrame(out Radius, out Bor);
		if (form is Window window)
		{
			SetSize(window.Size);
			SetLocation(window.Location);
			((Form)this).Size = window.Size;
			((Form)this).Location = window.Location;
		}
		else
		{
			SetSize(form.Size);
			SetLocation(form.Location);
			((Form)this).Size = form.Size;
			((Form)this).Location = form.Location;
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		if (form is Window window)
		{
			SetSize(window.Size);
			SetLocation(window.Location);
			((Form)this).Size = window.Size;
			((Form)this).Location = window.Location;
		}
		else
		{
			SetSize(form.Size);
			SetLocation(form.Location);
			((Form)this).Size = form.Size;
			((Form)this).Location = form.Location;
		}
		((Control)form).LocationChanged += Form_LSChanged;
		((Control)form).SizeChanged += Form_LSChanged;
		base.OnLoad(e);
	}

	private void Form_LSChanged(object? sender, EventArgs e)
	{
		if (form is Window window)
		{
			SetSize(window.Size);
			SetLocation(window.Location);
			((Form)this).Size = window.Size;
			((Form)this).Location = window.Location;
		}
		else
		{
			SetSize(form.Size);
			SetLocation(form.Location);
			((Form)this).Size = form.Size;
			((Form)this).Location = form.Location;
		}
		Bitmap? obj = temp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		temp = null;
		Print();
	}

	protected override void Dispose(bool disposing)
	{
		((Control)form).LocationChanged -= Form_LSChanged;
		((Control)form).SizeChanged -= Form_LSChanged;
		Bitmap? obj = temp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		temp = null;
		base.Dispose(disposing);
	}

	public override Bitmap PrintBit()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rect = (HasBor ? new Rectangle(Bor, 0, targetRectXY.Width - Bor * 2, targetRectXY.Height - Bor) : targetRectXY);
		if (temp == null || ((Image)temp).Width != targetRectXY.Width || ((Image)temp).Height != targetRectXY.Height)
		{
			Bitmap? obj = temp;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp = new Bitmap(targetRectXY.Width, targetRectXY.Height);
			using Canvas canvas = Graphics.FromImage((Image)(object)temp).High();
			SolidBrush val = new SolidBrush(Color.FromArgb(115, 0, 0, 0));
			try
			{
				if (Radius > 0)
				{
					GraphicsPath val2 = rect.RoundPath(Radius);
					try
					{
						canvas.Fill((Brush)(object)val, val2);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				else
				{
					canvas.Fill((Brush)(object)val, rect);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return (Bitmap)((Image)temp).Clone();
	}
}
