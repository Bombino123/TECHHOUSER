using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormDrawer : ILayeredForm
{
	private int FrmRadius;

	private int FrmBor;

	private bool HasBor;

	private Drawer.Config config;

	private int padding = 24;

	private ILayeredForm? formMask;

	public bool isclose;

	private bool vertical;

	private Bitmap? tempContent;

	private int start_X;

	private int end_X;

	private int start_Y;

	private int end_Y;

	private int start_W;

	private int end_W;

	private int start_H;

	private int end_H;

	private ITask? task_start;

	private bool run_end;

	private bool ok_end;

	private Form? form;

	private bool isok = true;

	internal bool LoadEnd = true;

	internal Action? LoadOK;

	private Bitmap? shadow_temp;

	public LayeredFormDrawer(Drawer.Config _config, ILayeredForm mask)
		: this(_config)
	{
		formMask = mask;
		if (config.MaskClosable)
		{
			((Control)mask).Click += delegate
			{
				isclose = true;
				IClose();
			};
		}
	}

	public LayeredFormDrawer(Drawer.Config _config)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected O, but got Unknown
		config = _config;
		((Control?)(object)config.Form).SetTopMost(((Control)this).Handle);
		((Control)this).Font = ((Control)config.Form).Font;
		padding = (int)Math.Round((float)config.Padding * Config.Dpi);
		((Control)this).Padding = new Padding(padding);
		HasBor = config.Form.FormFrame(out FrmRadius, out FrmBor);
		config.Content.BackColor = Colour.BgElevated.Get("Drawer");
		config.Content.ForeColor = Colour.Text.Get("Drawer");
		SetPoint();
		SetSize(start_W, start_H);
		SetLocation(start_X, start_Y);
		if (vertical)
		{
			tempContent = new Bitmap(end_W - padding * 2, end_H - 20 - padding * 2);
		}
		else
		{
			tempContent = new Bitmap(end_W - 20 - padding * 2, end_H - padding * 2);
		}
		if (!(config.Content.Tag is Size))
		{
			config.Content.Tag = config.Content.Size;
			Helper.DpiAuto(Config.Dpi, config.Content);
		}
		config.Content.Location = new Point(-((Image)tempContent).Width * 2, -((Image)tempContent).Height * 2);
		config.Content.Size = new Size(((Image)tempContent).Width, ((Image)tempContent).Height);
		LoadContent();
		config.Content.DrawToBitmap(tempContent, new Rectangle(0, 0, ((Image)tempContent).Width, ((Image)tempContent).Height));
		((Control)config.Form).LocationChanged += Form_LocationChanged;
		((Control)config.Form).SizeChanged += Form_SizeChanged;
	}

	private void SetPoint()
	{
		switch (config.Align)
		{
		case TAlignMini.Top:
			vertical = true;
			start_H = 0;
			end_H = (int)((float)config.Content.Height * Config.Dpi) + padding * 2 + 20;
			if (config.Form is Window window3)
			{
				start_W = (end_W = window3.Width);
				start_X = (end_X = window3.Left);
				start_Y = (end_Y = window3.Top);
			}
			else
			{
				start_W = (end_W = ((Control)config.Form).Width);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			break;
		case TAlignMini.Bottom:
			vertical = true;
			start_H = 0;
			end_H = (int)((float)config.Content.Height * Config.Dpi) + padding * 2 + 20;
			if (config.Form is Window window2)
			{
				start_W = (end_W = window2.Width);
				start_X = (end_X = window2.Left);
				start_Y = window2.Top + window2.Height;
			}
			else
			{
				start_W = (end_W = ((Control)config.Form).Width);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = ((Control)config.Form).Top + ((Control)config.Form).Height;
			}
			end_Y = start_Y - end_H;
			break;
		case TAlignMini.Left:
			start_W = 0;
			end_W = (int)((float)config.Content.Width * Config.Dpi) + padding * 2 + 20;
			if (config.Form is Window window4)
			{
				start_H = (end_H = window4.Height);
				start_X = (end_X = window4.Left);
				start_Y = (end_Y = window4.Top);
			}
			else
			{
				start_H = (end_H = ((Control)config.Form).Height);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			break;
		default:
			start_W = 0;
			end_W = (int)((float)config.Content.Width * Config.Dpi) + padding * 2 + 20;
			if (config.Form is Window window)
			{
				start_H = (end_H = window.Height);
				start_X = window.Left + window.Width;
				start_Y = (end_Y = window.Top);
			}
			else
			{
				start_H = (end_H = ((Control)config.Form).Height);
				start_X = ((Control)config.Form).Left + ((Control)config.Form).Width;
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			end_X = start_X - end_W;
			break;
		}
	}

	private void Form_SizeChanged(object? sender, EventArgs e)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)config.Form.WindowState == 1)
		{
			return;
		}
		switch (config.Align)
		{
		case TAlignMini.Top:
			if (config.Form is Window window3)
			{
				start_W = (end_W = window3.Width);
				start_X = (end_X = window3.Left);
				start_Y = (end_Y = window3.Top);
			}
			else
			{
				start_W = (end_W = ((Control)config.Form).Width);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			break;
		case TAlignMini.Bottom:
			if (config.Form is Window window2)
			{
				start_W = (end_W = window2.Width);
				start_X = (end_X = window2.Left);
				start_Y = window2.Top + window2.Height;
			}
			else
			{
				start_W = (end_W = ((Control)config.Form).Width);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = ((Control)config.Form).Top + ((Control)config.Form).Height;
			}
			end_Y = start_Y - end_H;
			break;
		case TAlignMini.Left:
			if (config.Form is Window window4)
			{
				start_H = (end_H = window4.Height);
				start_X = (end_X = window4.Left);
				start_Y = (end_Y = window4.Top);
			}
			else
			{
				start_H = (end_H = ((Control)config.Form).Height);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			break;
		default:
			if (config.Form is Window window)
			{
				start_H = (end_H = window.Height);
				start_X = window.Left + window.Width;
				start_Y = (end_Y = window.Top);
			}
			else
			{
				start_H = (end_H = ((Control)config.Form).Height);
				start_X = ((Control)config.Form).Left + ((Control)config.Form).Width;
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			end_X = start_X - end_W;
			break;
		}
		if (task_start == null)
		{
			SetLocation(end_X, end_Y);
			SetSize(end_W, end_H);
			if (form != null)
			{
				isok = false;
				Rectangle rectangle = Ang();
				form.Location = rectangle.Location;
				form.Size = rectangle.Size;
				isok = true;
			}
			Print();
		}
	}

	private void Form_LocationChanged(object? sender, EventArgs e)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)config.Form.WindowState == 1)
		{
			SetLocation(-end_W * 2, -end_H * 2);
			if (task_start == null)
			{
				if (form != null)
				{
					form.Location = new Point(-((Control)form).Width * 2, -((Control)form).Height * 2);
				}
				Print();
			}
			return;
		}
		switch (config.Align)
		{
		case TAlignMini.Top:
			if (config.Form is Window window3)
			{
				start_W = (end_W = window3.Width);
				start_X = (end_X = window3.Left);
				start_Y = (end_Y = window3.Top);
			}
			else
			{
				start_W = (end_W = ((Control)config.Form).Width);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			break;
		case TAlignMini.Bottom:
			if (config.Form is Window window2)
			{
				start_W = (end_W = window2.Width);
				start_X = (end_X = window2.Left);
				start_Y = window2.Top + window2.Height;
			}
			else
			{
				start_W = (end_W = ((Control)config.Form).Width);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = ((Control)config.Form).Top + ((Control)config.Form).Height;
			}
			end_Y = start_Y - end_H;
			break;
		case TAlignMini.Left:
			if (config.Form is Window window4)
			{
				start_H = (end_H = window4.Height);
				start_X = (end_X = window4.Left);
				start_Y = (end_Y = window4.Top);
			}
			else
			{
				start_H = (end_H = ((Control)config.Form).Height);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			break;
		default:
			if (config.Form is Window window)
			{
				start_H = (end_H = window.Height);
				start_X = window.Left + window.Width;
				start_Y = (end_Y = window.Top);
			}
			else
			{
				start_H = (end_H = ((Control)config.Form).Height);
				start_X = ((Control)config.Form).Left + ((Control)config.Form).Width;
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			end_X = start_X - end_W;
			break;
		}
		if (task_start == null)
		{
			SetLocation(end_X, end_Y);
			if (form != null)
			{
				form.Location = Ang().Location;
			}
			Print();
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		if (Config.Animation)
		{
			int t = Animation.TotalFrames(10, 100);
			int sleep = (config.Mask ? 200 : 0);
			task_start = new ITask(vertical ? ((Func<int, bool>)delegate(int i)
			{
				float num2 = Animation.Animate(i, t, 1f, AnimationType.Ball);
				SetAnimateValueY(start_Y + (int)((float)(end_Y - start_Y) * num2), (int)((float)end_H * num2), (byte)(255f * num2));
				return true;
			}) : ((Func<int, bool>)delegate(int i)
			{
				float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
				SetAnimateValueX(start_X + (int)((float)(end_X - start_X) * num), (int)((float)end_W * num), (byte)(255f * num));
				return true;
			}), 10, t, delegate
			{
				if (((Control)this).IsHandleCreated)
				{
					((Control)this).BeginInvoke((Delegate)new Action(ShowContent));
				}
				SetAnimateValue(end_X, end_Y, end_W, end_H, byte.MaxValue);
				task_start = null;
			}, sleep);
			base.OnLoad(e);
		}
		else
		{
			SetAnimateValue(end_X, end_Y, end_W, end_H, byte.MaxValue);
			base.OnLoad(e);
			ShowContent();
		}
	}

	private void LoadContent()
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		Rectangle rectangle = Ang();
		Point location = new Point(-rectangle.Width * 2, -rectangle.Height * 2);
		Control content = config.Content;
		Form val = (Form)(object)((content is Form) ? content : null);
		if (val != null)
		{
			((Control)val).BackColor = Colour.BgElevated.Get("Drawer");
			val.FormBorderStyle = (FormBorderStyle)0;
			val.Location = location;
			val.ClientSize = rectangle.Size;
			form = val;
		}
		else
		{
			DoubleBufferForm doubleBufferForm = new DoubleBufferForm((Form)(object)this, config.Content);
			((Control)doubleBufferForm).BackColor = Colour.BgElevated.Get("Drawer");
			((Form)doubleBufferForm).FormBorderStyle = (FormBorderStyle)0;
			((Form)doubleBufferForm).Location = location;
			((Form)doubleBufferForm).ClientSize = rectangle.Size;
			form = (Form?)(object)doubleBufferForm;
		}
		if (!config.Dispose)
		{
			object tag = config.Content.Tag;
			if (tag is Size)
			{
				Size size = (Size)tag;
				form.FormClosing += (FormClosingEventHandler)delegate
				{
					config.Content.Dock = (DockStyle)0;
					config.Content.Size = size;
					config.Content.Location = new Point(-config.Content.Width * 2, -config.Content.Height * 2);
					((Control)config.Form).Controls.Add(config.Content);
				};
			}
		}
		((Component)(object)config.Content).Disposed += delegate
		{
			config.Content.SizeChanged -= Content_SizeChanged;
			((Form)this).Close();
		};
		form.Show((IWin32Window)(object)this);
		form.Location = location;
		form.ClientSize = rectangle.Size;
	}

	private void ShowContent()
	{
		if (form != null)
		{
			Rectangle rectangle = Ang();
			if (form.ClientSize != rectangle.Size)
			{
				form.ClientSize = rectangle.Size;
			}
			form.Location = rectangle.Location;
			config.OnLoad?.Invoke();
			LoadOK?.Invoke();
			if (config.Content is DrawerLoad drawerLoad)
			{
				drawerLoad.LoadOK();
			}
			LoadEnd = false;
			config.Content.SizeChanged += Content_SizeChanged;
			Bitmap? obj = tempContent;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			tempContent = null;
		}
	}

	private void Content_SizeChanged(object? sender, EventArgs e)
	{
		if (form == null || !isok)
		{
			return;
		}
		isok = false;
		Size size = config.Content.Size;
		switch (config.Align)
		{
		case TAlignMini.Top:
			end_H = (int)((float)size.Height * Config.Dpi) + padding * 2 + 20;
			if (config.Form is Window window3)
			{
				start_W = (end_W = window3.Width);
				start_X = (end_X = window3.Left);
				start_Y = (end_Y = window3.Top);
			}
			else
			{
				start_W = (end_W = ((Control)config.Form).Width);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			break;
		case TAlignMini.Bottom:
			end_H = (int)((float)size.Height * Config.Dpi) + padding * 2 + 20;
			if (config.Form is Window window2)
			{
				start_W = (end_W = window2.Width);
				start_X = (end_X = window2.Left);
				start_Y = window2.Top + window2.Height;
			}
			else
			{
				start_W = (end_W = ((Control)config.Form).Width);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = ((Control)config.Form).Top + ((Control)config.Form).Height;
			}
			end_Y = start_Y - end_H;
			break;
		case TAlignMini.Left:
			end_W = (int)((float)size.Width * Config.Dpi) + padding * 2 + 20;
			if (config.Form is Window window4)
			{
				start_H = (end_H = window4.Height);
				start_X = (end_X = window4.Left);
				start_Y = (end_Y = window4.Top);
			}
			else
			{
				start_H = (end_H = ((Control)config.Form).Height);
				start_X = (end_X = ((Control)config.Form).Left);
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			break;
		default:
			end_W = (int)((float)size.Width * Config.Dpi) + padding * 2 + 20;
			if (config.Form is Window window)
			{
				start_H = (end_H = window.Height);
				start_X = window.Left + window.Width;
				start_Y = (end_Y = window.Top);
			}
			else
			{
				start_H = (end_H = ((Control)config.Form).Height);
				start_X = ((Control)config.Form).Left + ((Control)config.Form).Width;
				start_Y = (end_Y = ((Control)config.Form).Top);
			}
			end_X = start_X - end_W;
			break;
		}
		SetLocation(end_X, end_Y);
		SetSize(end_W, end_H);
		Rectangle rectangle = Ang();
		form.Location = rectangle.Location;
		form.Size = rectangle.Size;
		Print();
		ITask.Run(delegate
		{
			Thread.Sleep(500);
			isok = true;
		});
	}

	private Rectangle Ang()
	{
		return config.Align switch
		{
			TAlignMini.Top => new Rectangle(end_X + padding, end_Y + padding, end_W - padding * 2, end_H - 20 - padding * 2), 
			TAlignMini.Bottom => new Rectangle(end_X + padding, end_Y + padding + 20, end_W - padding * 2, end_H - 20 - padding * 2), 
			TAlignMini.Left => new Rectangle(end_X + padding, end_Y + padding, end_W - 20 - padding * 2, end_H - padding * 2), 
			_ => new Rectangle(end_X + padding + 20, end_Y + padding, end_W - 20 - padding * 2, end_H - padding * 2), 
		};
	}

	private void SetAnimateValueX(int x, int w, byte _alpha)
	{
		if (base.TargetRect.X != x || base.TargetRect.Width != w || alpha != _alpha)
		{
			SetLocationX(x);
			SetSizeW(w);
			alpha = _alpha;
			Print();
		}
	}

	private void SetAnimateValueY(int y, int h, byte _alpha)
	{
		if (base.TargetRect.Y != y || base.TargetRect.Height != h || alpha != _alpha)
		{
			SetLocationY(y);
			SetSizeH(h);
			alpha = _alpha;
			Print();
		}
	}

	private void SetAnimateValue(int x, int y, int w, int h, byte _alpha)
	{
		if (base.TargetRect.X != x || base.TargetRect.Y != y || base.TargetRect.Width != w || base.TargetRect.Height != h || alpha != _alpha)
		{
			SetLocation(x, y);
			SetSize(w, h);
			alpha = _alpha;
			Print();
		}
	}

	protected override void DestroyHandle()
	{
		((Control)this).DestroyHandle();
		isclose = true;
		formMask?.IClose();
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		task_start?.Dispose();
		if (!ok_end)
		{
			((Control)config.Form).LocationChanged -= Form_LocationChanged;
			((Control)config.Form).SizeChanged -= Form_SizeChanged;
			tempContent = new Bitmap(config.Content.Width, config.Content.Height);
			config.Content.DrawToBitmap(tempContent, new Rectangle(0, 0, ((Image)tempContent).Width, ((Image)tempContent).Height));
			if (form != null)
			{
				form.Location = new Point(-((Control)form).Width * 2, -((Control)form).Height * 2);
			}
			e.Cancel = true;
			if (Config.Animation)
			{
				if (!run_end)
				{
					run_end = true;
					int t = Animation.TotalFrames(10, 100);
					new ITask(vertical ? ((Func<int, bool>)delegate(int i)
					{
						float num2 = Animation.Animate(i, t, 1f, AnimationType.Ball);
						SetAnimateValueY(end_Y - (int)((float)(end_Y - start_Y) * num2), (int)((float)end_H * (1f - num2)), (byte)(255f * (1f - num2)));
						return true;
					}) : ((Func<int, bool>)delegate(int i)
					{
						float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
						SetAnimateValueX(end_X - (int)((float)(end_X - start_X) * num), (int)((float)end_W * (1f - num)), (byte)(255f * (1f - num)));
						return true;
					}), 10, t, delegate
					{
						ok_end = true;
						IClose(isdispose: true);
					});
				}
			}
			else
			{
				ok_end = true;
				IClose(isdispose: true);
			}
		}
		((Form)this).OnClosing(e);
	}

	protected override void Dispose(bool disposing)
	{
		((Control)config.Form).LocationChanged -= Form_LocationChanged;
		((Control)config.Form).SizeChanged -= Form_SizeChanged;
		if (config.Dispose)
		{
			((Component)(object)config.Content).Dispose();
		}
		Bitmap? obj = tempContent;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		((Component)(object)form)?.Dispose();
		task_start?.Dispose();
		config.OnClose?.Invoke();
		config.OnClose = null;
		base.Dispose(disposing);
	}

	public void IRClose()
	{
		ok_end = true;
		IClose(isdispose: true);
	}

	public override Bitmap PrintBit()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		Rectangle rect = ((!HasBor) ? base.TargetRectXY : new Rectangle(FrmBor, 0, base.TargetRect.Width - FrmBor * 2, base.TargetRect.Height - FrmBor));
		Bitmap val = new Bitmap(base.TargetRect.Width, base.TargetRect.Height);
		using (Canvas canvas = Graphics.FromImage((Image)(object)val).High())
		{
			Rectangle rect2 = DrawShadow(canvas, rect);
			GraphicsPath val2 = rect2.RoundPath(FrmRadius);
			try
			{
				canvas.Fill(Colour.BgElevated.Get("Drawer"), val2);
				if (tempContent != null)
				{
					canvas.Image(tempContent, new Rectangle(rect2.X + padding, rect2.Y + padding, ((Image)tempContent).Width, ((Image)tempContent).Height));
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		return val;
	}

	private Rectangle DrawShadow(Canvas g, Rectangle rect)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Expected O, but got Unknown
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Expected O, but got Unknown
		ColorMatrix val = new ColorMatrix
		{
			Matrix33 = 0.3f
		};
		switch (config.Align)
		{
		case TAlignMini.Top:
			if (Config.ShadowEnabled)
			{
				if (shadow_temp == null || ((Image)shadow_temp).Width != end_W)
				{
					Bitmap? obj2 = shadow_temp;
					if (obj2 != null)
					{
						((Image)obj2).Dispose();
					}
					GraphicsPath val4 = new Rectangle(rect.X, rect.Y + 20, end_W, 40).RoundPath(FrmRadius);
					try
					{
						shadow_temp = val4.PaintShadow(end_W, 80, 20);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				ImageAttributes val5 = new ImageAttributes();
				try
				{
					val5.SetColorMatrix(val, (ColorMatrixFlag)0, (ColorAdjustType)1);
					g.Image((Image)(object)shadow_temp, new Rectangle(rect.Y, rect.Bottom - 80, rect.Width, 80), 0, 0, ((Image)shadow_temp).Width, ((Image)shadow_temp).Height, (GraphicsUnit)2, val5);
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
			}
			return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - 20);
		case TAlignMini.Bottom:
			if (Config.ShadowEnabled)
			{
				if (shadow_temp == null || ((Image)shadow_temp).Width != end_W)
				{
					Bitmap? obj4 = shadow_temp;
					if (obj4 != null)
					{
						((Image)obj4).Dispose();
					}
					GraphicsPath val8 = new Rectangle(rect.X, rect.Y + 20, end_W, 40).RoundPath(FrmRadius);
					try
					{
						shadow_temp = val8.PaintShadow(end_W, 80, 20);
					}
					finally
					{
						((IDisposable)val8)?.Dispose();
					}
				}
				ImageAttributes val9 = new ImageAttributes();
				try
				{
					val9.SetColorMatrix(val, (ColorMatrixFlag)0, (ColorAdjustType)1);
					g.Image((Image)(object)shadow_temp, new Rectangle(rect.Y, rect.Y, rect.Width, 80), 0, 0, ((Image)shadow_temp).Width, ((Image)shadow_temp).Height, (GraphicsUnit)2, val9);
				}
				finally
				{
					((IDisposable)val9)?.Dispose();
				}
			}
			return new Rectangle(rect.X, rect.Y + 20, rect.Width, rect.Height - 20);
		case TAlignMini.Left:
			if (Config.ShadowEnabled)
			{
				if (shadow_temp == null || ((Image)shadow_temp).Height != end_H)
				{
					Bitmap? obj3 = shadow_temp;
					if (obj3 != null)
					{
						((Image)obj3).Dispose();
					}
					GraphicsPath val6 = new Rectangle(rect.X + 20, rect.Y, 40, end_H).RoundPath(FrmRadius);
					try
					{
						shadow_temp = val6.PaintShadow(80, end_H, 20);
					}
					finally
					{
						((IDisposable)val6)?.Dispose();
					}
				}
				ImageAttributes val7 = new ImageAttributes();
				try
				{
					val7.SetColorMatrix(val, (ColorMatrixFlag)0, (ColorAdjustType)1);
					g.Image((Image)(object)shadow_temp, new Rectangle(rect.Right - 80, rect.Y, 80, rect.Height), 0, 0, ((Image)shadow_temp).Width, ((Image)shadow_temp).Height, (GraphicsUnit)2, val7);
				}
				finally
				{
					((IDisposable)val7)?.Dispose();
				}
			}
			return new Rectangle(rect.X, rect.Y, rect.Width - 20, rect.Height);
		default:
			if (Config.ShadowEnabled)
			{
				if (shadow_temp == null || ((Image)shadow_temp).Height != end_H)
				{
					Bitmap? obj = shadow_temp;
					if (obj != null)
					{
						((Image)obj).Dispose();
					}
					GraphicsPath val2 = new Rectangle(rect.X + 20, rect.Y, 40, end_H).RoundPath(FrmRadius);
					try
					{
						shadow_temp = val2.PaintShadow(80, end_H, 20);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				ImageAttributes val3 = new ImageAttributes();
				try
				{
					val3.SetColorMatrix(val, (ColorMatrixFlag)0, (ColorAdjustType)1);
					g.Image((Image)(object)shadow_temp, new Rectangle(rect.X, rect.Y, 80, rect.Height), 0, 0, ((Image)shadow_temp).Width, ((Image)shadow_temp).Height, (GraphicsUnit)2, val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			return new Rectangle(rect.X + 20, rect.Y, rect.Width - 20, rect.Height);
		}
	}
}
