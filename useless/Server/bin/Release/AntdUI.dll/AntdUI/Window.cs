using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Vanara.PInvoke;

namespace AntdUI;

public class Window : BaseForm, IMessageFilter
{
	private bool resizable = true;

	private WState winState;

	private bool ReadMessage;

	private bool _isaddMessage;

	private static IntPtr TRUE = new IntPtr(1);

	private static IntPtr FALSE = new IntPtr(0);

	private bool iszoomed;

	private int oldmargin = -1;

	public static bool CanHandMessage = true;

	private const nint SIZE_RESTORED = 0;

	private const nint SIZE_MINIMIZED = 1;

	private const nint SIZE_MAXIMIZED = 2;

	internal Size? sizeInit;

	private Size? sizeNormal;

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
				}
				EventHub.Dispatch(EventType.WINDOW_STATE, winState == WState.Maximize);
			}
		}
	}

	protected virtual bool UseMessageFilter => false;

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
			if (((Control)this).IsHandleCreated)
			{
				Size maximumSize = ((Control)this).MaximumSize;
				Size minimumSize = ((Control)this).MinimumSize;
				Size maximumSize2 = (((Control)this).MinimumSize = ((Form)this).ClientSize);
				((Control)this).MaximumSize = maximumSize2;
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
				((Control)this).MinimumSize = minimumSize;
				((Control)this).MaximumSize = maximumSize;
				oldmargin = 0;
				DwmArea();
			}
			else
			{
				((Form)this).ShowInTaskbar = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Point Location
	{
		get
		{
			if (winState == WState.Restore)
			{
				return ((Form)this).Location;
			}
			return ScreenRectangle.Location;
		}
		set
		{
			sizeNormal = null;
			((Form)this).Location = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Top
	{
		get
		{
			return Location.Y;
		}
		set
		{
			sizeNormal = null;
			((Control)this).Top = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Left
	{
		get
		{
			return Location.X;
		}
		set
		{
			sizeNormal = null;
			((Control)this).Left = value;
		}
	}

	public int Right => ScreenRectangle.Right;

	public int Bottom => ScreenRectangle.Bottom;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Size Size
	{
		get
		{
			if (winState == WState.Restore)
			{
				return ((Form)this).Size;
			}
			return ScreenRectangle.Size;
		}
		set
		{
			sizeNormal = null;
			((Form)this).Size = value;
			sizeInit = ((Form)this).ClientSize;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Width
	{
		get
		{
			return Size.Width;
		}
		set
		{
			sizeNormal = null;
			((Control)this).Width = value;
			sizeInit = ((Form)this).ClientSize;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Height
	{
		get
		{
			return Size.Height;
		}
		set
		{
			sizeNormal = null;
			((Control)this).Height = value;
			sizeInit = ((Form)this).ClientSize;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Rectangle ScreenRectangle
	{
		get
		{
			if (winState == WState.Restore)
			{
				return new Rectangle(((Form)this).Location, ((Form)this).Size);
			}
			Rectangle clientRectangle = ((Control)this).ClientRectangle;
			return new Rectangle(((Control)this).RectangleToScreen(Rectangle.Empty).Location, clientRectangle.Size);
		}
		set
		{
			sizeNormal = null;
			((Form)this).Location = value.Location;
			((Form)this).Size = value.Size;
			sizeInit = ((Form)this).ClientSize;
		}
	}

	public override bool IsMax => winState == WState.Maximize;

	private void HandMessage()
	{
		ReadMessage = winState == WState.Restore && resizable;
		if (UseMessageFilter)
		{
			IsAddMessage = true;
		}
		else
		{
			IsAddMessage = ReadMessage;
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		((Form)this).OnHandleCreated(e);
		SetTheme();
		User32.DisableProcessWindowsGhosting();
		if ((int)base.FormBorderStyle != 0 && (int)((Form)this).WindowState != 2)
		{
			sizeInit = ((Form)this).ClientSize;
			SetSize(sizeInit.Value);
		}
		HandMessage();
		DwmArea();
	}

	private void SetSize(Size size)
	{
		Size maximumSize = ((Control)this).MaximumSize;
		Size minimumSize = ((Control)this).MinimumSize;
		Size size3 = (((Form)this).ClientSize = size);
		Size maximumSize2 = (((Control)this).MinimumSize = size3);
		((Control)this).MaximumSize = maximumSize2;
		((Control)this).MinimumSize = minimumSize;
		((Control)this).MaximumSize = maximumSize;
	}

	protected override void OnLoad(EventArgs e)
	{
		User32.SetWindowPos(((Control)this).Handle, HWND.NULL, 0, 0, 0, 0, User32.SetWindowPosFlags.SWP_DRAWFRAME | User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOOWNERZORDER | User32.SetWindowPosFlags.SWP_NOSIZE | User32.SetWindowPosFlags.SWP_NOZORDER);
		base.OnLoad(e);
	}

	private void InvalidateNonclient()
	{
		if (((Control)this).IsHandleCreated && !((Control)this).IsDisposed)
		{
			User32.RedrawWindow(((Control)this).Handle, null, HWND.NULL, User32.RedrawWindowFlags.RDW_VALIDATE | User32.RedrawWindowFlags.RDW_UPDATENOW | User32.RedrawWindowFlags.RDW_FRAME);
			User32.UpdateWindow(((Control)this).Handle);
			User32.SetWindowPos(((Control)this).Handle, HWND.NULL, 0, 0, 0, 0, User32.SetWindowPosFlags.SWP_DRAWFRAME | User32.SetWindowPosFlags.SWP_NOACTIVATE | User32.SetWindowPosFlags.SWP_NOCOPYBITS | User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOOWNERZORDER | User32.SetWindowPosFlags.SWP_NOSIZE | User32.SetWindowPosFlags.SWP_NOZORDER);
		}
	}

	protected override void WndProc(ref Message m)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		User32.WindowMessage msg = (User32.WindowMessage)((Message)(ref m)).Msg;
		if (msg <= User32.WindowMessage.WM_ACTIVATE)
		{
			if (msg != User32.WindowMessage.WM_SIZE)
			{
				if (msg != User32.WindowMessage.WM_ACTIVATE)
				{
					goto IL_0061;
				}
				DwmArea();
			}
			else
			{
				WmSize(ref m);
			}
			goto IL_0070;
		}
		if (msg == User32.WindowMessage.WM_ACTIVATEAPP)
		{
			goto IL_0059;
		}
		if (msg != User32.WindowMessage.WM_NCCALCSIZE)
		{
			if (msg == User32.WindowMessage.WM_NCACTIVATE)
			{
				goto IL_0059;
			}
		}
		else if (((Message)(ref m)).WParam != IntPtr.Zero)
		{
			if (WmNCCalcSize(ref m))
			{
				return;
			}
			goto IL_0070;
		}
		goto IL_0061;
		IL_0070:
		((Form)this).WndProc(ref m);
		return;
		IL_0059:
		InvalidateNonclient();
		goto IL_0070;
		IL_0061:
		if (WmGhostingHandler(m))
		{
			return;
		}
		goto IL_0070;
	}

	private bool WmGhostingHandler(Message m)
	{
		int msg = ((Message)(ref m)).Msg;
		if ((uint)(msg - 174) <= 1u || msg == 49596)
		{
			((Message)(ref m)).Result = FALSE;
			InvalidateNonclient();
		}
		return false;
	}

	private bool ISZoomed()
	{
		bool flag = User32.IsZoomed(((Control)this).Handle);
		if (iszoomed == flag)
		{
			return flag;
		}
		iszoomed = flag;
		DwmArea();
		return flag;
	}

	private void DwmArea()
	{
		int num = ((!iszoomed && !IsFull) ? 1 : 0);
		if (oldmargin != num)
		{
			oldmargin = num;
			HWND hWnd = ((Control)this).Handle;
			DwmApi.MARGINS pMarInset = new DwmApi.MARGINS(num);
			DwmApi.DwmExtendFrameIntoClientArea(hWnd, in pMarInset);
		}
	}

	public override void RefreshDWM()
	{
		DwmArea();
	}

	public override bool ResizableMouseMove()
	{
		User32.HitTestValues hitTestValues = HitTest(((Control)this).PointToClient(Control.MousePosition));
		if (hitTestValues != 0)
		{
			User32.HitTestValues hitTestValues2 = hitTestValues;
			if (hitTestValues2 != User32.HitTestValues.HTCLIENT && winState == WState.Restore)
			{
				SetCursorHit(hitTestValues2);
				return true;
			}
		}
		return false;
	}

	public override bool ResizableMouseMove(Point point)
	{
		User32.HitTestValues hitTestValues = HitTest(point);
		if (hitTestValues != 0)
		{
			User32.HitTestValues hitTestValues2 = hitTestValues;
			if (hitTestValues2 != User32.HitTestValues.HTCLIENT && winState == WState.Restore)
			{
				SetCursorHit(hitTestValues2);
				return true;
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
		if (CanHandMessage && ReadMessage)
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

	private void WmSize(ref Message m)
	{
		if (((Message)(ref m)).WParam == (IntPtr)1)
		{
			WinState = WState.Minimize;
		}
		else if (((Message)(ref m)).WParam == (IntPtr)2)
		{
			WinState = WState.Maximize;
			((Control)this).Invalidate();
			InvalidateNonclient();
		}
		else if (((Message)(ref m)).WParam == (IntPtr)0)
		{
			sizeNormal = ((Form)this).ClientSize;
			WinState = WState.Restore;
			InvalidateNonclient();
			((Control)this).Invalidate();
		}
	}

	private bool WmNCCalcSize(ref Message m)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((int)base.FormBorderStyle == 0)
		{
			return false;
		}
		if (ISZoomed())
		{
			RECT structure = Marshal.PtrToStructure<RECT>(((Message)(ref m)).LParam);
			Padding nonClientMetrics = GetNonClientMetrics();
			structure.top -= ((Padding)(ref nonClientMetrics)).Top;
			structure.top += ((Padding)(ref nonClientMetrics)).Bottom;
			Marshal.StructureToPtr(structure, ((Message)(ref m)).LParam, fDeleteOld: false);
			((Message)(ref m)).Result = new IntPtr(1024);
			return false;
		}
		((Message)(ref m)).Result = TRUE;
		return true;
	}

	protected override void SetClientSizeCore(int x, int y)
	{
		if (((Component)(object)this).DesignMode)
		{
			Size = new Size(x, y);
		}
		else
		{
			((Form)this).SetClientSizeCore(x, y);
		}
	}

	protected Padding GetNonClientMetrics()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		clientRectangle.Offset(-((Control)this).Bounds.Left, -((Control)this).Bounds.Top);
		RECT lpRect = new RECT(clientRectangle);
		User32.AdjustWindowRectEx(ref lpRect, (User32.WindowStyles)((Control)this).CreateParams.Style, bMenu: false, (User32.WindowStylesEx)((Control)this).CreateParams.ExStyle);
		Padding result = default(Padding);
		((Padding)(ref result)).Top = clientRectangle.Top - lpRect.top;
		((Padding)(ref result)).Left = clientRectangle.Left - lpRect.left;
		((Padding)(ref result)).Bottom = lpRect.bottom - clientRectangle.Bottom;
		((Padding)(ref result)).Right = lpRect.right - clientRectangle.Right;
		return result;
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)(object)this).DesignMode)
		{
			((Form)this).SetBoundsCore(x, y, width, height, specified);
		}
		else if ((int)((Form)this).WindowState == 0 && sizeNormal.HasValue)
		{
			((Form)this).SetBoundsCore(x, y, sizeNormal.Value.Width, sizeNormal.Value.Height, (BoundsSpecified)0);
		}
		else
		{
			((Form)this).SetBoundsCore(x, y, width, height, specified);
		}
	}
}
