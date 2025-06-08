using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Vanara.PInvoke;

namespace AntdUI;

public class BaseForm : Form
{
	private bool dark;

	private TAMode mode;

	private FormBorderStyle formBorderStyle = (FormBorderStyle)4;

	public bool IsFull;

	internal bool is_resizable;

	internal Action? ONESC;

	[Description("深色模式")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Dark
	{
		get
		{
			return dark;
		}
		set
		{
			if (dark != value)
			{
				dark = value;
				mode = ((!dark) ? TAMode.Light : TAMode.Dark);
				if (((Control)this).IsHandleCreated)
				{
					DarkUI.UseImmersiveDarkMode(((Control)this).Handle, value);
				}
			}
		}
	}

	[Description("色彩模式")]
	[Category("外观")]
	[DefaultValue(TAMode.Auto)]
	public TAMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				if (mode == TAMode.Dark || mode == TAMode.Auto || Config.Mode == TMode.Dark)
				{
					Dark = true;
				}
				else
				{
					Dark = false;
				}
			}
		}
	}

	[Description("指示窗体的边框和标题栏的外观和行为")]
	[Category("行为")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public FormBorderStyle FormBorderStyle
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return formBorderStyle;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (formBorderStyle != value)
			{
				((Form)this).FormBorderStyle = (formBorderStyle = value);
			}
		}
	}

	public virtual bool IsMax => (int)((Form)this).WindowState == 2;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public virtual bool AutoHandDpi { get; set; } = true;


	[Description("鼠标拖拽大小使能")]
	[Category("交互")]
	[DefaultValue(true)]
	public bool EnableHitTest { get; set; } = true;


	public BaseForm()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).SetStyle((ControlStyles)204802, true);
		((Control)this).UpdateStyles();
	}

	public void SetCursor(bool val)
	{
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				SetCursor(val);
			});
		}
		else
		{
			((Control)this).Cursor = (val ? Cursors.Hand : ((Control)this).DefaultCursor);
		}
	}

	internal void SetTheme()
	{
		if (mode == TAMode.Dark || mode == TAMode.Auto || Config.Mode == TMode.Dark)
		{
			DarkUI.UseImmersiveDarkMode(((Control)this).Handle, enabled: true);
		}
	}

	public virtual void RefreshDWM()
	{
	}

	public virtual void Min()
	{
		((Form)this).WindowState = (FormWindowState)1;
	}

	public virtual bool MaxRestore()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (IsFull)
		{
			((Form)this).FormBorderStyle = formBorderStyle;
			IsFull = false;
		}
		if ((int)((Form)this).WindowState == 2)
		{
			((Form)this).WindowState = (FormWindowState)0;
			RefreshDWM();
			return false;
		}
		((Form)this).WindowState = (FormWindowState)2;
		RefreshDWM();
		return true;
	}

	public virtual void Max()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (IsFull)
		{
			((Form)this).FormBorderStyle = formBorderStyle;
			IsFull = false;
		}
		((Form)this).WindowState = (FormWindowState)2;
		RefreshDWM();
	}

	public virtual bool FullRestore()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)((Form)this).WindowState == 2)
		{
			NoFull();
			return false;
		}
		Full();
		return true;
	}

	public virtual void Full()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		if (!IsFull)
		{
			IsFull = true;
			((Form)this).FormBorderStyle = (FormBorderStyle)0;
			if ((int)((Form)this).WindowState == 2)
			{
				((Form)this).WindowState = (FormWindowState)0;
			}
			((Form)this).WindowState = (FormWindowState)2;
			RefreshDWM();
		}
	}

	public virtual void NoFull()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (IsFull)
		{
			IsFull = false;
			((Form)this).FormBorderStyle = formBorderStyle;
			((Form)this).WindowState = (FormWindowState)0;
			RefreshDWM();
		}
		else if (IsMax)
		{
			MaxRestore();
		}
	}

	public float Dpi()
	{
		return Config.Dpi;
	}

	protected override void OnLoad(EventArgs e)
	{
		if (AutoHandDpi)
		{
			AutoDpi(Dpi(), (Control)(object)this);
		}
		((Form)this).OnLoad(e);
	}

	public void AutoDpi(Control control)
	{
		AutoDpi(Dpi(), control);
	}

	public void AutoDpi(float dpi, Control control)
	{
		Helper.DpiAuto(dpi, control);
	}

	public virtual void DraggableMouseDown()
	{
		if (!IsFull)
		{
			User32.ReleaseCapture();
			User32.SendMessage(((Control)this).Handle, 274, 61458, IntPtr.Zero);
		}
	}

	public virtual bool ResizableMouseMove()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((Form)this).WindowState == 0)
		{
			User32.HitTestValues hitTestValues = HitTest(((Control)this).PointToClient(Control.MousePosition));
			if (hitTestValues != 0)
			{
				User32.HitTestValues hitTestValues2 = hitTestValues;
				if (hitTestValues2 != User32.HitTestValues.HTCLIENT)
				{
					SetCursorHit(hitTestValues2);
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool ResizableMouseMove(Point point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((Form)this).WindowState == 0)
		{
			User32.HitTestValues hitTestValues = HitTest(point);
			if (hitTestValues != 0)
			{
				User32.HitTestValues hitTestValues2 = hitTestValues;
				if (hitTestValues2 != User32.HitTestValues.HTCLIENT)
				{
					SetCursorHit(hitTestValues2);
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool ResizableMouseDown()
	{
		Point mousePosition = Control.MousePosition;
		User32.HitTestValues hitTestValues = HitTest(((Control)this).PointToClient(mousePosition));
		if (hitTestValues != User32.HitTestValues.HTCLIENT)
		{
			is_resizable = true;
			SetCursorHit(hitTestValues);
			User32.ReleaseCapture();
			User32.SendMessage(((Control)this).Handle, 161u, (IntPtr)(int)hitTestValues, Macros.MAKELPARAM(mousePosition.X, mousePosition.Y));
			is_resizable = false;
			return true;
		}
		return false;
	}

	internal User32.HitTestValues HitTest(Point point)
	{
		if (EnableHitTest)
		{
			float num = 8f * Config.Dpi;
			float num2 = num * 2f;
			User32.GetWindowRect(((Control)this).Handle, out var lpRect);
			Rectangle rectangle = new Rectangle(Point.Empty, lpRect.Size);
			User32.HitTestValues result = User32.HitTestValues.HTCLIENT;
			int x = point.X;
			int y = point.Y;
			if ((float)x < (float)rectangle.Left + num2 && (float)y < (float)rectangle.Top + num2)
			{
				result = User32.HitTestValues.HTTOPLEFT;
			}
			else if ((float)x >= (float)rectangle.Left + num2 && (float)x <= (float)rectangle.Right - num2 && (float)y <= (float)rectangle.Top + num)
			{
				result = User32.HitTestValues.HTTOP;
			}
			else if ((float)x > (float)rectangle.Right - num2 && (float)y <= (float)rectangle.Top + num2)
			{
				result = User32.HitTestValues.HTTOPRIGHT;
			}
			else if ((float)x <= (float)rectangle.Left + num && (float)y >= (float)rectangle.Top + num2 && (float)y <= (float)rectangle.Bottom - num2)
			{
				result = User32.HitTestValues.HTLEFT;
			}
			else if ((float)x >= (float)rectangle.Right - num && (float)y >= (float)(rectangle.Top * 2) + num && (float)y <= (float)rectangle.Bottom - num2)
			{
				result = User32.HitTestValues.HTRIGHT;
			}
			else if ((float)x <= (float)rectangle.Left + num2 && (float)y >= (float)rectangle.Bottom - num2)
			{
				result = User32.HitTestValues.HTBOTTOMLEFT;
			}
			else if ((float)x > (float)rectangle.Left + num2 && (float)x < (float)rectangle.Right - num2 && (float)y >= (float)rectangle.Bottom - num)
			{
				result = User32.HitTestValues.HTBOTTOM;
			}
			else if ((float)x >= (float)rectangle.Right - num2 && (float)y >= (float)rectangle.Bottom - num2)
			{
				result = User32.HitTestValues.HTBOTTOMRIGHT;
			}
			return result;
		}
		return User32.HitTestValues.HTCLIENT;
	}

	internal void SetCursorHit(User32.HitTestValues mode)
	{
		switch (mode)
		{
		case User32.HitTestValues.HTTOP:
		case User32.HitTestValues.HTBOTTOM:
			LoadCursors(32645);
			break;
		case User32.HitTestValues.HTLEFT:
		case User32.HitTestValues.HTRIGHT:
			LoadCursors(32644);
			break;
		case User32.HitTestValues.HTTOPLEFT:
		case User32.HitTestValues.HTBOTTOMRIGHT:
			LoadCursors(32642);
			break;
		case User32.HitTestValues.HTTOPRIGHT:
		case User32.HitTestValues.HTBOTTOMLEFT:
			LoadCursors(32643);
			break;
		}
	}

	internal void LoadCursors(int id)
	{
		ResourceId lpCursorName = Macros.MAKEINTRESOURCE(id);
		User32.SetCursor(User32.LoadCursor(default(HINSTANCE), lpCursorName)).Close();
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		if (ONESC == null)
		{
			return ((Form)this).ProcessDialogKey(keyData);
		}
		if ((keyData & 0x60000) == 0 && (keyData & 0xFFFF) == 27)
		{
			ONESC();
			return true;
		}
		return ((Form)this).ProcessDialogKey(keyData);
	}
}
