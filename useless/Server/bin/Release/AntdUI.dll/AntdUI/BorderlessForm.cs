using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Vanara.PInvoke;

namespace AntdUI;

public class BorderlessForm : BaseForm, IMessageFilter
{
	private int shadow = 10;

	private Color shadowColor = Color.FromArgb(100, 0, 0, 0);

	private int borderWidth = 1;

	private Color borderColor = Color.FromArgb(180, 0, 0, 0);

	private int radius;

	private bool DwmEnabled;

	private BorderlessFormShadow? skin;

	private readonly IntPtr TRUE = new IntPtr(1);

	private int oldmargin;

	private const nint SIZE_RESTORED = 0;

	private const nint SIZE_MINIMIZED = 1;

	private const nint SIZE_MAXIMIZED = 2;

	private bool resizable = true;

	private WState winState;

	private bool ReadMessage;

	private bool _isaddMessage;

	private bool ismax;

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new FormBorderStyle FormBorderStyle => base.FormBorderStyle;

	private bool CanMessageFilter
	{
		get
		{
			if (!DwmEnabled)
			{
				return shadow < 4;
			}
			return true;
		}
	}

	[Description("阴影大小")]
	[Category("外观")]
	[DefaultValue(10)]
	public int Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (shadow == value)
			{
				return;
			}
			shadow = value;
			if (DwmEnabled)
			{
				return;
			}
			if (value > 0)
			{
				ShowSkin();
				skin?.ISize();
				skin?.ClearShadow();
				skin?.Print();
				return;
			}
			BorderlessFormShadow? borderlessFormShadow = skin;
			if (borderlessFormShadow != null)
			{
				((Form)borderlessFormShadow).Close();
			}
			skin = null;
		}
	}

	[Description("使用DWM阴影")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool UseDwm { get; set; } = true;


	[Description("鼠标穿透")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ShadowPierce { get; set; }

	[Description("阴影颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "100, 0, 0, 0")]
	public Color ShadowColor
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
				skin?.ClearShadow();
				skin?.Print();
			}
		}
	}

	[Description("边框宽度")]
	[Category("外观")]
	[DefaultValue(1)]
	public int BorderWidth
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
				skin?.Print();
			}
		}
	}

	[Description("边框颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "180, 0, 0, 0")]
	public Color BorderColor
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
				skin?.Print();
			}
		}
	}

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(0)]
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
				SetReion();
				skin?.ClearShadow();
				skin?.Print();
			}
		}
	}

	[Description("确定窗体是否出现在 Windows 任务栏中")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool ShowInTaskbar
	{
		get
		{
			return ((Form)this).ShowInTaskbar;
		}
		set
		{
			if (((Form)this).ShowInTaskbar == value)
			{
				return;
			}
			if (((Control)this).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					((Form)this).ShowInTaskbar = value;
				});
			}
			else
			{
				((Form)this).ShowInTaskbar = value;
			}
			oldmargin = 0;
			DwmArea();
		}
	}

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = ((Form)this).CreateParams;
			createParams.Style |= 0x20000;
			return createParams;
		}
	}

	[Description("调整窗口大小")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool Resizable
	{
		get
		{
			return resizable;
		}
		set
		{
			if (resizable != value)
			{
				resizable = value;
				HandMessage();
			}
		}
	}

	private WState WinState
	{
		set
		{
			if (winState != value)
			{
				winState = value;
				if (((Control)this).IsHandleCreated)
				{
					HandMessage();
					SetReion();
				}
			}
		}
	}

	private bool IsAddMessage
	{
		set
		{
			if (_isaddMessage != value)
			{
				_isaddMessage = value;
				if (value)
				{
					Application.AddMessageFilter((IMessageFilter)(object)this);
				}
				else
				{
					Application.RemoveMessageFilter((IMessageFilter)(object)this);
				}
			}
		}
	}

	private bool isMax
	{
		get
		{
			return ismax;
		}
		set
		{
			if (ismax == value)
			{
				return;
			}
			ismax = value;
			if (value)
			{
				if (skin != null)
				{
					((Control)skin).Visible = false;
				}
			}
			else
			{
				ShowSkin();
			}
			DwmArea();
		}
	}

	public BorderlessForm()
	{
		((Control)this).SetStyle((ControlStyles)204818, true);
		((Control)this).UpdateStyles();
		base.FormBorderStyle = (FormBorderStyle)0;
	}

	protected override void OnCreateControl()
	{
		((Form)this).OnCreateControl();
		SetReion();
		ShowSkin();
	}

	protected override void OnClosed(EventArgs e)
	{
		((Form)this).OnClosed(e);
		BorderlessFormShadow? borderlessFormShadow = skin;
		if (borderlessFormShadow != null)
		{
			((Form)borderlessFormShadow).Close();
		}
		skin = null;
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		if (((Control)this).Visible && shadow > 0 && !((Component)(object)this).DesignMode)
		{
			ShowSkin();
		}
		else if (skin != null)
		{
			((Control)skin).Visible = false;
		}
		((Form)this).OnVisibleChanged(e);
	}

	private void ShowSkin()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!DwmEnabled && ((Control)this).Visible && (int)((Form)this).WindowState == 0 && shadow > 0 && !((Component)(object)this).DesignMode)
		{
			if (skin != null)
			{
				((Control)skin).Visible = true;
				return;
			}
			skin = new BorderlessFormShadow(this);
			((Form)skin).Show((IWin32Window)(object)this);
		}
	}

	protected override void OnLocationChanged(EventArgs e)
	{
		skin?.OnLocationChange();
		((Control)this).OnLocationChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		skin?.OnSizeChange();
		SetReion();
		((Control)this).OnSizeChanged(e);
	}

	protected override void WndProc(ref Message m)
	{
		switch ((User32.WindowMessage)((Message)(ref m)).Msg)
		{
		case User32.WindowMessage.WM_ERASEBKGND:
			((Message)(ref m)).Result = IntPtr.Zero;
			return;
		case User32.WindowMessage.WM_ACTIVATE:
		case User32.WindowMessage.WM_NCPAINT:
			DwmArea();
			break;
		case User32.WindowMessage.WM_NCHITTEST:
			((Message)(ref m)).Result = TRUE;
			return;
		case User32.WindowMessage.WM_SIZE:
			WmSize(ref m);
			break;
		case User32.WindowMessage.WM_NCMOUSEMOVE:
		case User32.WindowMessage.WM_MOUSEFIRST:
			if (!is_resizable && ReadMessage)
			{
				ResizableMouseMove(((Control)this).PointToClient(Control.MousePosition));
			}
			break;
		case User32.WindowMessage.WM_NCLBUTTONDOWN:
		case User32.WindowMessage.WM_LBUTTONDOWN:
			if (!is_resizable && ReadMessage)
			{
				ResizableMouseDown();
			}
			break;
		}
		((Form)this).WndProc(ref m);
	}

	private void DwmArea()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (DwmEnabled && shadow > 0)
		{
			int num = (((int)((Form)this).WindowState == 0) ? 1 : 0);
			if (oldmargin != num)
			{
				oldmargin = num;
				int attrValue = 2;
				DarkUI.DwmSetWindowAttribute(((Control)this).Handle, 2, ref attrValue, 4);
				HWND hWnd = ((Control)this).Handle;
				DwmApi.MARGINS pMarInset = new DwmApi.MARGINS(num);
				DwmApi.DwmExtendFrameIntoClientArea(hWnd, in pMarInset);
			}
		}
	}

	private void WmSize(ref Message m)
	{
		if (((Message)(ref m)).WParam == (IntPtr)1)
		{
			WinState = WState.Minimize;
		}
		else if (((Message)(ref m)).WParam == (IntPtr)2)
		{
			WinState = WState.Maximize;
		}
		else if (((Message)(ref m)).WParam == (IntPtr)0)
		{
			WinState = WState.Restore;
		}
	}

	private void SetReion()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		if (((Control)this).Region != null)
		{
			((Control)this).Region.Dispose();
		}
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width <= 0 || clientRectangle.Height <= 0)
		{
			return;
		}
		if (IsMax)
		{
			((Control)this).Region = new Region(clientRectangle);
		}
		else if (!UseDwm || !OS.Win11)
		{
			GraphicsPath val = clientRectangle.RoundPath((float)radius * Config.Dpi);
			try
			{
				Region val2 = new Region(val);
				val.Widen(Pens.White);
				val2.Union(val);
				((Control)this).Region = val2;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private void HandMessage()
	{
		ReadMessage = CanMessageFilter && winState == WState.Restore && resizable;
		IsAddMessage = ReadMessage;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Form)this).OnHandleCreated(e);
		if (UseDwm && OS.Version.Major >= 6)
		{
			try
			{
				int pfEnabled = 0;
				DarkUI.DwmIsCompositionEnabled(ref pfEnabled);
				DwmEnabled = pfEnabled == 1;
			}
			catch
			{
			}
		}
		SetTheme();
		User32.DisableProcessWindowsGhosting();
		HandMessage();
	}

	public override void DraggableMouseDown()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		if (IsFull)
		{
			return;
		}
		Point mouseOffset = Control.MousePosition;
		bool end = true;
		bool handmax = false;
		Size minimumSize = ((Control)this).MinimumSize;
		Size maximumSize = ((Control)this).MaximumSize;
		if (DwmEnabled && (int)((Form)this).WindowState == 2)
		{
			ITask.Run(delegate
			{
				while (end)
				{
					Point mousePosition2 = Control.MousePosition;
					if (mouseOffset != mousePosition2)
					{
						if (Math.Abs(mousePosition2.X - mouseOffset.X) >= 6 || Math.Abs(mousePosition2.Y - mouseOffset.Y) >= 6)
						{
							handmax = true;
							((Control)this).Invoke((Delegate)(Action)delegate
							{
								((Form)this).WindowState = (FormWindowState)0;
								isMax = false;
							});
							break;
						}
					}
					else
					{
						Thread.Sleep(10);
					}
				}
			});
		}
		User32.ReleaseCapture();
		User32.SendMessage(((Control)this).Handle, 274, 61458, IntPtr.Zero);
		end = false;
		if (handmax)
		{
			((Control)this).MaximumSize = maximumSize;
			((Control)this).MinimumSize = minimumSize;
			return;
		}
		Point mousePosition = Control.MousePosition;
		Screen val = Screen.FromPoint(mousePosition);
		if (mousePosition.Y == val.WorkingArea.Top && ((Form)this).MaximizeBox)
		{
			Max();
		}
	}

	public override bool ResizableMouseMove()
	{
		if (winState == WState.Restore)
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

	public override bool ResizableMouseMove(Point point)
	{
		if (winState == WState.Restore)
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

	public bool PreFilterMessage(ref Message m)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (is_resizable)
		{
			return OnPreFilterMessage(m);
		}
		if (Window.CanHandMessage && ReadMessage)
		{
			switch (((Message)(ref m)).Msg)
			{
			case 160:
			case 512:
				if (isMe(((Message)(ref m)).HWnd) && ResizableMouseMove(((Control)this).PointToClient(Control.MousePosition)))
				{
					return true;
				}
				break;
			case 161:
			case 513:
				if (isMe(((Message)(ref m)).HWnd) && ResizableMouseDown())
				{
					return true;
				}
				break;
			}
		}
		return OnPreFilterMessage(m);
	}

	protected virtual bool OnPreFilterMessage(Message m)
	{
		return false;
	}

	private bool isMe(IntPtr intPtr)
	{
		Control val = Control.FromHandle(intPtr);
		if (val == this || GetParent(val) == this)
		{
			return true;
		}
		return false;
	}

	private static Control? GetParent(Control? control)
	{
		try
		{
			if (control != null && control.IsHandleCreated && control.Parent != null)
			{
				if (control is Form)
				{
					return control;
				}
				return GetParent(control.Parent);
			}
		}
		catch
		{
		}
		return control;
	}

	public override void RefreshDWM()
	{
		DwmArea();
	}

	public override bool MaxRestore()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		IsFull = false;
		if ((int)((Form)this).WindowState == 2)
		{
			((Form)this).WindowState = (FormWindowState)0;
			isMax = false;
			RefreshDWM();
			return false;
		}
		Screen val = Screen.FromPoint(((Form)this).Location);
		if (val.Primary)
		{
			((Form)this).MaximizedBounds = val.WorkingArea;
		}
		else
		{
			((Form)this).MaximizedBounds = new Rectangle(0, 0, 0, 0);
		}
		((Form)this).WindowState = (FormWindowState)2;
		isMax = true;
		RefreshDWM();
		return true;
	}

	public override void Max()
	{
		if (!ismax)
		{
			IsFull = false;
			Screen val = Screen.FromPoint(((Form)this).Location);
			if (val.Primary)
			{
				((Form)this).MaximizedBounds = val.WorkingArea;
			}
			else
			{
				((Form)this).MaximizedBounds = new Rectangle(0, 0, 0, 0);
			}
			((Form)this).WindowState = (FormWindowState)2;
			isMax = true;
			RefreshDWM();
		}
	}

	public override bool FullRestore()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)((Form)this).WindowState == 2)
		{
			NoFull();
			isMax = false;
			return false;
		}
		Full();
		isMax = true;
		return true;
	}

	public override void Full()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if (!IsFull)
		{
			if ((int)((Form)this).WindowState == 2)
			{
				((Form)this).WindowState = (FormWindowState)0;
			}
			((Form)this).MaximizedBounds = new Rectangle(0, 0, 0, 0);
			((Form)this).WindowState = (FormWindowState)2;
			bool isFull = (isMax = true);
			IsFull = isFull;
			RefreshDWM();
		}
	}

	public override void NoFull()
	{
		if (IsFull)
		{
			bool isFull = (isMax = false);
			IsFull = isFull;
			((Form)this).WindowState = (FormWindowState)0;
			RefreshDWM();
		}
	}
}
