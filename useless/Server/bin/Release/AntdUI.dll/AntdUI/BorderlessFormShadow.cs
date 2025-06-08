using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class BorderlessFormShadow : Form
{
	private BorderlessForm form;

	private Rectangle shadow_rect = new Rectangle(0, 0, 0, 0);

	private Rectangle rect_read;

	private Bitmap? bitbmp;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = ((Form)this).CreateParams;
			createParams.ExStyle |= 0x8080000;
			if (form != null && form.ShadowPierce)
			{
				createParams.ExStyle |= 0x20;
			}
			createParams.Parent = IntPtr.Zero;
			return createParams;
		}
	}

	protected override bool ShowWithoutActivation => true;

	public BorderlessFormShadow(BorderlessForm main)
	{
		form = main;
		((Control)this).SetStyle((ControlStyles)204818, true);
		((Control)this).UpdateStyles();
		((Form)this).ShowInTaskbar = false;
		((Form)this).FormBorderStyle = (FormBorderStyle)0;
		((Form)this).Icon = ((Form)form).Icon;
		((Form)this).ShowIcon = false;
		((Control)this).Text = ((Control)form).Text;
		ISize();
	}

	protected override void WndProc(ref Message m)
	{
		if (form.Resizable)
		{
			switch (((Message)(ref m)).Msg)
			{
			case 33:
				((Message)(ref m)).Result = new IntPtr(3);
				return;
			case 160:
			case 512:
				if (form.ResizableMouseMove(((Control)this).PointToClient(Control.MousePosition)))
				{
					return;
				}
				break;
			case 161:
			case 513:
				if (form.ResizableMouseDown())
				{
					return;
				}
				break;
			}
		}
		((Form)this).WndProc(ref m);
	}

	public void ClearShadow()
	{
		Bitmap? obj = bitbmp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		bitbmp = null;
	}

	public void OnSizeChange()
	{
		if (form.IsMax)
		{
			((Control)this).Visible = false;
			return;
		}
		if (((Control)form).Visible)
		{
			((Control)this).Visible = true;
		}
		ISize();
		Print();
	}

	public void OnLocationChange()
	{
		ISize();
		Print();
	}

	protected override void OnCreateControl()
	{
		((Form)this).OnCreateControl();
		ISize();
		Print();
	}

	public void ISize()
	{
		int num = (int)((float)form.Shadow * Config.Dpi);
		int num2 = num * 2;
		rect_read = new Rectangle(num, num, ((Control)form).Width, ((Control)form).Height);
		shadow_rect = new Rectangle(((Control)form).Left - num, ((Control)form).Top - num, rect_read.Width + num2, rect_read.Height + num2);
	}

	public void Print()
	{
		if (!((Control)this).IsHandleCreated || shadow_rect.Width <= 0 || shadow_rect.Height <= 0)
		{
			return;
		}
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)new Action(Print));
			return;
		}
		try
		{
			Bitmap val = PrintBit();
			try
			{
				if (val == null)
				{
					return;
				}
				Win32.SetBits(val, shadow_rect, ((Control)this).Handle);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			GC.Collect();
		}
		catch
		{
		}
	}

	private Bitmap PrintBit()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		int num = (int)((float)form.Radius * Config.Dpi);
		int num2 = (int)((float)form.Shadow * Config.Dpi);
		int num3 = num2 * 2;
		int num4 = num2 * 4;
		int num5 = num2 * 6;
		if (bitbmp == null)
		{
			bitbmp = new Bitmap(num5, num5);
			using Canvas canvas = Graphics.FromImage((Image)(object)bitbmp).High();
			GraphicsPath val = new Rectangle(num2, num2, num5 - num3, num5 - num3).RoundPath(num);
			try
			{
				canvas.Fill(form.ShadowColor, val);
				Helper.Blur(bitbmp, num2);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		Bitmap val2 = new Bitmap(shadow_rect.Width, shadow_rect.Height);
		using (Canvas canvas2 = Graphics.FromImage((Image)(object)val2).High())
		{
			GraphicsPath val3 = rect_read.RoundPath(num);
			try
			{
				GraphicsPath val4 = new GraphicsPath();
				try
				{
					val4.AddPath(val3, false);
					val4.AddRectangle(new Rectangle(0, 0, shadow_rect.Width, shadow_rect.Height));
					canvas2.SetClip(val4);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
				canvas2.Image((Image)(object)bitbmp, new Rectangle(0, 0, num3, num3), new Rectangle(0, 0, num3, num3), (GraphicsUnit)2);
				canvas2.Image((Image)(object)bitbmp, new Rectangle(0, num3, num2, ((Image)val2).Height - num4), new Rectangle(0, num3, num2, ((Image)bitbmp).Height - num4), (GraphicsUnit)2);
				canvas2.Image((Image)(object)bitbmp, new Rectangle(0, ((Image)val2).Height - num3, num3, num3), new Rectangle(0, ((Image)bitbmp).Height - num3, num3, num3), (GraphicsUnit)2);
				canvas2.Image((Image)(object)bitbmp, new Rectangle(num3, ((Image)val2).Height - num2, ((Image)val2).Width - num4, num2), new Rectangle(num3, ((Image)bitbmp).Height - num2, ((Image)bitbmp).Width - num4, num2), (GraphicsUnit)2);
				canvas2.Image((Image)(object)bitbmp, new Rectangle(((Image)val2).Width - num3, ((Image)val2).Height - num3, num3, num3), new Rectangle(((Image)bitbmp).Width - num3, ((Image)bitbmp).Height - num3, num3, num3), (GraphicsUnit)2);
				canvas2.Image((Image)(object)bitbmp, new Rectangle(((Image)val2).Width - num2, num3, num2, ((Image)val2).Height - num4), new Rectangle(((Image)bitbmp).Width - num2, num3, num2, ((Image)bitbmp).Height - num4), (GraphicsUnit)2);
				canvas2.Image((Image)(object)bitbmp, new Rectangle(((Image)val2).Width - num3, 0, num3, num3), new Rectangle(((Image)bitbmp).Width - num3, 0, num3, num3), (GraphicsUnit)2);
				canvas2.Image((Image)(object)bitbmp, new Rectangle(num3, 0, ((Image)val2).Width - num4, num2), new Rectangle(num3, 0, ((Image)bitbmp).Width - num4, num2), (GraphicsUnit)2);
				canvas2.ResetClip();
				if (form.BorderWidth > 0)
				{
					canvas2.Draw(form.BorderColor, (float)form.BorderWidth * Config.Dpi, val3);
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		return val2;
	}
}
