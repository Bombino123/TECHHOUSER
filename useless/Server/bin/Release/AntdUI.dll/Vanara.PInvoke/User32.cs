using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Vanara.PInvoke;

public static class User32
{
	public enum WindowMessage
	{
		WM_NULL = 0,
		WM_CREATE = 1,
		WM_DESTROY = 2,
		WM_MOVE = 3,
		WM_SIZE = 5,
		WM_ACTIVATE = 6,
		WM_SETFOCUS = 7,
		WM_KILLFOCUS = 8,
		WM_ENABLE = 10,
		WM_SETREDRAW = 11,
		WM_SETTEXT = 12,
		WM_GETTEXT = 13,
		WM_GETTEXTLENGTH = 14,
		WM_PAINT = 15,
		WM_CLOSE = 16,
		WM_QUERYENDSESSION = 17,
		WM_QUERYOPEN = 19,
		WM_ENDSESSION = 22,
		WM_QUIT = 18,
		WM_ERASEBKGND = 20,
		WM_SYSCOLORCHANGE = 21,
		WM_SHOWWINDOW = 24,
		WM_WININICHANGE = 26,
		WM_SETTINGCHANGE = 26,
		WM_DEVMODECHANGE = 27,
		WM_ACTIVATEAPP = 28,
		WM_FONTCHANGE = 29,
		WM_TIMECHANGE = 30,
		WM_CANCELMODE = 31,
		WM_SETCURSOR = 32,
		WM_MOUSEACTIVATE = 33,
		WM_CHILDACTIVATE = 34,
		WM_QUEUESYNC = 35,
		WM_GETMINMAXINFO = 36,
		WM_PAINTICON = 38,
		WM_ICONERASEBKGND = 39,
		WM_NEXTDLGCTL = 40,
		WM_SPOOLERSTATUS = 42,
		WM_DRAWITEM = 43,
		WM_MEASUREITEM = 44,
		WM_DELETEITEM = 45,
		WM_VKEYTOITEM = 46,
		WM_CHARTOITEM = 47,
		WM_SETFONT = 48,
		WM_GETFONT = 49,
		WM_SETHOTKEY = 50,
		WM_GETHOTKEY = 51,
		WM_QUERYDRAGICON = 55,
		WM_COMPAREITEM = 57,
		WM_GETOBJECT = 61,
		WM_COMPACTING = 65,
		[Obsolete]
		WM_COMMNOTIFY = 68,
		WM_WINDOWPOSCHANGING = 70,
		WM_WINDOWPOSCHANGED = 71,
		[Obsolete]
		WM_POWER = 72,
		WM_COPYDATA = 74,
		WM_CANCELJOURNAL = 75,
		WM_NOTIFY = 78,
		WM_INPUTLANGCHANGEREQUEST = 80,
		WM_INPUTLANGCHANGE = 81,
		WM_TCARD = 82,
		WM_HELP = 83,
		WM_USERCHANGED = 84,
		WM_NOTIFYFORMAT = 85,
		WM_CONTEXTMENU = 123,
		WM_STYLECHANGING = 124,
		WM_STYLECHANGED = 125,
		WM_DISPLAYCHANGE = 126,
		WM_GETICON = 127,
		WM_SETICON = 128,
		WM_NCCREATE = 129,
		WM_NCDESTROY = 130,
		WM_NCCALCSIZE = 131,
		WM_NCHITTEST = 132,
		WM_NCPAINT = 133,
		WM_NCACTIVATE = 134,
		WM_GETDLGCODE = 135,
		WM_SYNCPAINT = 136,
		WM_UAHDESTROYWINDOW = 144,
		WM_UAHDRAWMENU = 145,
		WM_UAHDRAWMENUITEM = 146,
		WM_UAHINITMENU = 147,
		WM_UAHMEASUREMENUITEM = 148,
		WM_UAHNCPAINTMENUPOPUP = 149,
		WM_NCMOUSEMOVE = 160,
		WM_NCLBUTTONDOWN = 161,
		WM_NCLBUTTONUP = 162,
		WM_NCLBUTTONDBLCLK = 163,
		WM_NCRBUTTONDOWN = 164,
		WM_NCRBUTTONUP = 165,
		WM_NCRBUTTONDBLCLK = 166,
		WM_NCMBUTTONDOWN = 167,
		WM_NCMBUTTONUP = 168,
		WM_NCMBUTTONDBLCLK = 169,
		WM_NCXBUTTONDOWN = 171,
		WM_NCXBUTTONUP = 172,
		WM_NCXBUTTONDBLCLK = 173,
		WM_BM_CLICK = 245,
		WM_INPUT_DEVICE_CHANGE = 254,
		WM_INPUT = 255,
		WM_KEYFIRST = 256,
		WM_KEYDOWN = 256,
		WM_KEYUP = 257,
		WM_CHAR = 258,
		WM_DEADCHAR = 259,
		WM_SYSKEYDOWN = 260,
		WM_SYSKEYUP = 261,
		WM_SYSCHAR = 262,
		WM_SYSDEADCHAR = 263,
		WM_UNICHAR = 265,
		WM_KEYLAST = 265,
		WM_IME_STARTCOMPOSITION = 269,
		WM_IME_ENDCOMPOSITION = 270,
		WM_IME_COMPOSITION = 271,
		WM_IME_KEYLAST = 271,
		WM_INITDIALOG = 272,
		WM_COMMAND = 273,
		WM_SYSCOMMAND = 274,
		WM_TIMER = 275,
		WM_HSCROLL = 276,
		WM_VSCROLL = 277,
		WM_INITMENU = 278,
		WM_INITMENUPOPUP = 279,
		WM_MENUSELECT = 287,
		WM_MENUCHAR = 288,
		WM_ENTERIDLE = 289,
		WM_MENURBUTTONUP = 290,
		WM_MENUDRAG = 291,
		WM_MENUGETOBJECT = 292,
		WM_UNINITMENUPOPUP = 293,
		WM_MENUCOMMAND = 294,
		WM_CHANGEUISTATE = 295,
		WM_UPDATEUISTATE = 296,
		WM_QUERYUISTATE = 297,
		WM_CTLCOLORMSGBOX = 306,
		WM_CTLCOLOREDIT = 307,
		WM_CTLCOLORLISTBOX = 308,
		WM_CTLCOLORBTN = 309,
		WM_CTLCOLORDLG = 310,
		WM_CTLCOLORSCROLLBAR = 311,
		WM_CTLCOLORSTATIC = 312,
		WM_MOUSEFIRST = 512,
		WM_MOUSEMOVE = 512,
		WM_LBUTTONDOWN = 513,
		WM_LBUTTONUP = 514,
		WM_LBUTTONDBLCLK = 515,
		WM_RBUTTONDOWN = 516,
		WM_RBUTTONUP = 517,
		WM_RBUTTONDBLCLK = 518,
		WM_MBUTTONDOWN = 519,
		WM_MBUTTONUP = 520,
		WM_MBUTTONDBLCLK = 521,
		WM_MOUSEWHEEL = 522,
		WM_XBUTTONDOWN = 523,
		WM_XBUTTONUP = 524,
		WM_XBUTTONDBLCLK = 525,
		WM_MOUSEHWHEEL = 526,
		WM_MOUSELAST = 526,
		WM_PARENTNOTIFY = 528,
		WM_ENTERMENULOOP = 529,
		WM_EXITMENULOOP = 530,
		WM_NEXTMENU = 531,
		WM_SIZING = 532,
		WM_CAPTURECHANGED = 533,
		WM_MOVING = 534,
		WM_POWERBROADCAST = 536,
		WM_DEVICECHANGE = 537,
		WM_MDICREATE = 544,
		WM_MDIDESTROY = 545,
		WM_MDIACTIVATE = 546,
		WM_MDIRESTORE = 547,
		WM_MDINEXT = 548,
		WM_MDIMAXIMIZE = 549,
		WM_MDITILE = 550,
		WM_MDICASCADE = 551,
		WM_MDIICONARRANGE = 552,
		WM_MDIGETACTIVE = 553,
		WM_MDISETMENU = 560,
		WM_ENTERSIZEMOVE = 561,
		WM_EXITSIZEMOVE = 562,
		WM_DROPFILES = 563,
		WM_MDIREFRESHMENU = 564,
		WM_IME_SETCONTEXT = 641,
		WM_IME_NOTIFY = 642,
		WM_IME_CONTROL = 643,
		WM_IME_COMPOSITIONFULL = 644,
		WM_IME_SELECT = 645,
		WM_IME_CHAR = 646,
		WM_IME_REQUEST = 648,
		WM_IME_KEYDOWN = 656,
		WM_IME_KEYUP = 657,
		WM_MOUSEHOVER = 673,
		WM_MOUSELEAVE = 675,
		WM_NCMOUSEHOVER = 672,
		WM_NCMOUSELEAVE = 674,
		WM_WTSSESSION_CHANGE = 689,
		WM_TABLET_FIRST = 704,
		WM_TABLET_LAST = 735,
		WM_DPICHANGED = 736,
		WM_CUT = 768,
		WM_COPY = 769,
		WM_PASTE = 770,
		WM_CLEAR = 771,
		WM_UNDO = 772,
		WM_RENDERFORMAT = 773,
		WM_RENDERALLFORMATS = 774,
		WM_DESTROYCLIPBOARD = 775,
		WM_DRAWCLIPBOARD = 776,
		WM_PAINTCLIPBOARD = 777,
		WM_VSCROLLCLIPBOARD = 778,
		WM_SIZECLIPBOARD = 779,
		WM_ASKCBFORMATNAME = 780,
		WM_CHANGECBCHAIN = 781,
		WM_HSCROLLCLIPBOARD = 782,
		WM_QUERYNEWPALETTE = 783,
		WM_PALETTEISCHANGING = 784,
		WM_PALETTECHANGED = 785,
		WM_HOTKEY = 786,
		WM_PRINT = 791,
		WM_PRINTCLIENT = 792,
		WM_APPCOMMAND = 793,
		WM_THEMECHANGED = 794,
		WM_CLIPBOARDUPDATE = 797,
		WM_DWMCOMPOSITIONCHANGED = 798,
		WM_DWMNCRENDERINGCHANGED = 799,
		WM_DWMCOLORIZATIONCOLORCHANGED = 800,
		WM_DWMWINDOWMAXIMIZEDCHANGE = 801,
		WM_GETTITLEBARINFOEX = 831,
		WM_HANDHELDFIRST = 856,
		WM_HANDHELDLAST = 863,
		WM_AFXFIRST = 864,
		WM_AFXLAST = 895,
		WM_PENWINFIRST = 896,
		WM_PENWINLAST = 911,
		WM_APP = 32768,
		WM_USER = 1024,
		WM_CPL_LAUNCH = 5120,
		WM_CPL_LAUNCHED = 5121,
		WM_REFLECT = 8192,
		WM_SYSTIMER = 280
	}

	[Flags]
	public enum WindowStyles : uint
	{
		WS_BORDER = 0x800000u,
		WS_CAPTION = 0xC00000u,
		WS_CHILD = 0x40000000u,
		WS_CLIPCHILDREN = 0x2000000u,
		WS_CLIPSIBLINGS = 0x4000000u,
		WS_DISABLED = 0x8000000u,
		WS_DLGFRAME = 0x400000u,
		WS_GROUP = 0x20000u,
		WS_HSCROLL = 0x100000u,
		WS_MAXIMIZE = 0x1000000u,
		WS_MAXIMIZEBOX = 0x10000u,
		WS_MINIMIZE = 0x20000000u,
		WS_MINIMIZEBOX = 0x20000u,
		WS_OVERLAPPED = 0u,
		WS_OVERLAPPEDWINDOW = 0xCF0000u,
		WS_POPUP = 0x80000000u,
		WS_POPUPWINDOW = 0x80880000u,
		WS_THICKFRAME = 0x40000u,
		WS_SYSMENU = 0x80000u,
		WS_TABSTOP = 0x10000u,
		WS_VISIBLE = 0x10000000u,
		WS_VSCROLL = 0x200000u,
		WS_TILED = 0u,
		WS_ICONIC = 0x20000000u,
		WS_SIZEBOX = 0x40000u,
		WS_TILEDWINDOW = 0xCF0000u,
		WS_CHILDWINDOW = 0x40000000u
	}

	[Flags]
	public enum WindowStylesEx : uint
	{
		WS_EX_ACCEPTFILES = 0x10u,
		WS_EX_APPWINDOW = 0x40000u,
		WS_EX_CLIENTEDGE = 0x200u,
		WS_EX_COMPOSITED = 0x2000000u,
		WS_EX_CONTEXTHELP = 0x400u,
		WS_EX_CONTROLPARENT = 0x10000u,
		WS_EX_DLGMODALFRAME = 1u,
		WS_EX_LAYERED = 0x80000u,
		WS_EX_LAYOUTRTL = 0x400000u,
		WS_EX_LEFT = 0u,
		WS_EX_LEFTSCROLLBAR = 0x4000u,
		WS_EX_LTRREADING = 0u,
		WS_EX_MDICHILD = 0x40u,
		WS_EX_NOACTIVATE = 0x8000000u,
		WS_EX_NOINHERITLAYOUT = 0x100000u,
		WS_EX_NOPARENTNOTIFY = 4u,
		WS_EX_NOREDIRECTIONBITMAP = 0x200000u,
		WS_EX_OVERLAPPEDWINDOW = 0x300u,
		WS_EX_PALETTEWINDOW = 0x188u,
		WS_EX_RIGHT = 0x1000u,
		WS_EX_RIGHTSCROLLBAR = 0u,
		WS_EX_RTLREADING = 0x2000u,
		WS_EX_STATICEDGE = 0x20000u,
		WS_EX_TOOLWINDOW = 0x80u,
		WS_EX_TOPMOST = 8u,
		WS_EX_TRANSPARENT = 0x20u,
		WS_EX_WINDOWEDGE = 0x100u
	}

	public class SafeHCURSOR : SafeHANDLE, IUserHandle, IHandle
	{
		public SafeHCURSOR(IntPtr preexistingHandle, bool ownsHandle = true)
			: base(preexistingHandle, ownsHandle)
		{
		}

		private SafeHCURSOR()
		{
		}

		public static implicit operator HCURSOR(SafeHCURSOR h)
		{
			return h.handle;
		}

		protected override bool InternalReleaseHandle()
		{
			return DestroyCursor(this);
		}

		IntPtr IHandle.DangerousGetHandle()
		{
			return DangerousGetHandle();
		}
	}

	public enum HitTestValues : short
	{
		HTBORDER = 18,
		HTBOTTOM = 15,
		HTBOTTOMLEFT = 16,
		HTBOTTOMRIGHT = 17,
		HTCAPTION = 2,
		HTCLIENT = 1,
		HTCLOSE = 20,
		HTERROR = -2,
		HTGROWBOX = 4,
		HTHELP = 21,
		HTHSCROLL = 6,
		HTLEFT = 10,
		HTMENU = 5,
		HTMAXBUTTON = 9,
		HTMINBUTTON = 8,
		HTNOWHERE = 0,
		HTREDUCE = 8,
		HTRIGHT = 11,
		HTSIZE = 4,
		HTSYSMENU = 3,
		HTTOP = 12,
		HTTOPLEFT = 13,
		HTTOPRIGHT = 14,
		HTTRANSPARENT = -1,
		HTVSCROLL = 7,
		HTZOOM = 9
	}

	public struct WINDOWPOS
	{
		public HWND hwnd;

		public HWND hwndInsertAfter;

		public int x;

		public int y;

		public int cx;

		public int cy;

		public SetWindowPosFlags flags;
	}

	[PInvokeData("winuser.h", MSDNShortId = "setwindowpos")]
	[Flags]
	public enum SetWindowPosFlags : uint
	{
		SWP_ASYNCWINDOWPOS = 0x4000u,
		SWP_DEFERERASE = 0x2000u,
		SWP_DRAWFRAME = 0x20u,
		SWP_FRAMECHANGED = 0x20u,
		SWP_HIDEWINDOW = 0x80u,
		SWP_NOACTIVATE = 0x10u,
		SWP_NOCOPYBITS = 0x100u,
		SWP_NOMOVE = 2u,
		SWP_NOOWNERZORDER = 0x200u,
		SWP_NOREDRAW = 8u,
		SWP_NOREPOSITION = 0x200u,
		SWP_NOSENDCHANGING = 0x400u,
		SWP_NOSIZE = 1u,
		SWP_NOZORDER = 4u,
		SWP_SHOWWINDOW = 0x40u
	}

	public struct PAINTSTRUCT
	{
		public IntPtr hdc;

		public bool fErase;

		public RECT rcPaint;

		public bool fRestore;

		public bool fIncUpdate;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public byte[] rgbReserved;
	}

	[PInvokeData("winuser.h", MSDNShortId = "c6cb7f74-237e-4d3e-a852-894da36e990c")]
	[Flags]
	public enum RedrawWindowFlags
	{
		RDW_INVALIDATE = 1,
		RDW_INTERNALPAINT = 2,
		RDW_ERASE = 4,
		RDW_VALIDATE = 8,
		RDW_NOINTERNALPAINT = 0x10,
		RDW_NOERASE = 0x20,
		RDW_NOCHILDREN = 0x40,
		RDW_ALLCHILDREN = 0x80,
		RDW_UPDATENOW = 0x100,
		RDW_ERASENOW = 0x200,
		RDW_FRAME = 0x400,
		RDW_NOFRAME = 0x800
	}

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "3b1e2699-7f5f-444d-9072-f2ca7c8fa511")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ClientToScreen(HWND hWnd, ref Point lpPoint);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[SecurityCritical]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ScreenToClient(HWND hWnd, [In][Out] ref Point lpPoint);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "clipcursor")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ClipCursor(in RECT lpRect);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "createcursor")]
	public static extern SafeHCURSOR CreateCursor(HINSTANCE hInst, int xHotSpot, int yHotSpot, int nWidth, int nHeight, IntPtr pvANDPlane, IntPtr pvXORPlane);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "destroycursor")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DestroyCursor(HCURSOR hCursor);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "getclipcursor")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetClipCursor(out RECT lpRect);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "getcursor")]
	public static extern SafeHCURSOR GetCursor();

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "getcursorpos")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetCursorPos(out Point lpPoint);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "getphysicalcursorpos")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetPhysicalCursorPos(out Point lpPoint);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "loadcursor")]
	public static extern SafeHCURSOR LoadCursor(HINSTANCE hInstance, string lpCursorName);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "loadcursor")]
	public static extern SafeHCURSOR LoadCursor([Optional] HINSTANCE hInstance, ResourceId lpCursorName);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "loadcursorfromfile")]
	public static extern SafeHCURSOR LoadCursorFromFile(string lpFileName);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "setcursor")]
	public static extern SafeHCURSOR SetCursor(SafeHCURSOR hCursor);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "setcursorpos")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetCursorPos(int X, int Y);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "setphysicalcursorpos")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetPhysicalCursorPos(int X, int Y);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "showcursor")]
	public static extern int ShowCursor([MarshalAs(UnmanagedType.Bool)] bool bShow);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h")]
	public static extern uint GetMessagePos();

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[PInvokeData("winuser.h")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool PostMessage([Optional] HWND hWnd, uint Msg, [Optional] IntPtr wParam, [Optional] IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "NF:winuser.SendMessage")]
	[SecurityCritical]
	public static extern IntPtr SendMessage(HWND hWnd, uint msg, [Optional][In] IntPtr wParam, [Optional][In][Out] IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "NF:winuser.SendMessage")]
	[SecurityCritical]
	public static extern IntPtr SendMessage(HWND hWnd, uint msg, [Optional][In] IntPtr wParam, string lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "NF:winuser.SendMessage")]
	[SecurityCritical]
	public static extern IntPtr SendMessage(HWND hWnd, uint msg, ref int wParam, [In][Out] StringBuilder lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "NF:winuser.SendMessage")]
	[SecurityCritical]
	public static extern IntPtr SendMessage(HWND hWnd, uint msg, [Optional][In] IntPtr wParam, [In][Out] StringBuilder lParam);

	[PInvokeData("winuser.h", MSDNShortId = "NF:winuser.SendMessage")]
	public static IntPtr SendMessage<TMsg>(HWND hWnd, TMsg msg, [Optional] IntPtr wParam, [Optional] IntPtr lParam) where TMsg : struct, IConvertible
	{
		return SendMessage(hWnd, Convert.ToUInt32(msg), IntPtr.Zero, IntPtr.Zero);
	}

	[PInvokeData("winuser.h", MSDNShortId = "NF:winuser.SendMessage")]
	public static IntPtr SendMessage<TMsg, TWP>(HWND hWnd, TMsg msg, TWP wParam, [Optional] IntPtr lParam) where TMsg : struct, IConvertible where TWP : struct, IConvertible
	{
		return SendMessage(hWnd, Convert.ToUInt32(msg), (IntPtr)Convert.ToInt64(wParam), IntPtr.Zero);
	}

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "adjustwindowrect")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool AdjustWindowRect(ref RECT lpRect, WindowStyles dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "adjustwindowrectex")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool AdjustWindowRectEx(ref RECT lpRect, WindowStyles dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, WindowStylesEx dwExStyle);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "destroywindow")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DestroyWindow(HWND hWnd);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("WinUser.h", MSDNShortId = "ms633519")]
	[SecurityCritical]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(HWND hWnd, out RECT lpRect);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "iszoomed")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsZoomed(HWND hWnd);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern IntPtr FindWindowEx(HWND hwnd1, HWND hwnd2, string lpsz1, string lpsz2);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint = true);

	[DllImport("user32.dll")]
	public static extern int BeginPaint(HWND hwnd, ref PAINTSTRUCT lpPaint);

	[DllImport("user32.dll")]
	public static extern int EndPaint(HWND hwnd, ref PAINTSTRUCT lpPaint);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "isiconic")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsIconic(HWND hWnd);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	[PInvokeData("winuser.h")]
	public static extern IntPtr DefWindowProc(HWND hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "c6cb7f74-237e-4d3e-a852-894da36e990c")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool RedrawWindow(HWND hWnd, [In] PRECT? lprcUpdate, HWND hrgnUpdate, RedrawWindowFlags flags);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "51a50f1f-7b4d-4acd-83a0-1877f5181766")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool UpdateWindow(HWND hWnd);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[PInvokeData("winuser.h", MSDNShortId = "setwindowpos")]
	[SecurityCritical]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

	[DllImport("user32.dll", ExactSpelling = true)]
	[PInvokeData("winuser.h", MSDNShortId = "")]
	public static extern void DisableProcessWindowsGhosting();
}
