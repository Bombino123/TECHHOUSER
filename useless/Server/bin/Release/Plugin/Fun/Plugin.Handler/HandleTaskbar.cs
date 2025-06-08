using System;
using System.Diagnostics;
using System.Text;

namespace Plugin.Handler;

public class HandleTaskbar
{
	private const string VistaStartMenuCaption = "Start";

	private static IntPtr vistaStartMenuWnd = IntPtr.Zero;

	public void Show()
	{
		try
		{
			SetVisibility(show: true);
		}
		catch
		{
		}
	}

	public void Hide()
	{
		try
		{
			SetVisibility(show: false);
		}
		catch
		{
		}
	}

	private static void SetVisibility(bool show)
	{
		IntPtr intPtr = Native.FindWindow("Shell_TrayWnd", null);
		IntPtr intPtr2 = Native.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)49175, "Start");
		if (intPtr2 == IntPtr.Zero)
		{
			intPtr2 = Native.FindWindow("Button", null);
			if (intPtr2 == IntPtr.Zero)
			{
				intPtr2 = GetVistaStartMenuWnd(intPtr);
			}
		}
		Native.ShowWindow(intPtr, show ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
		Native.ShowWindow(intPtr2, show ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
	}

	private static IntPtr GetVistaStartMenuWnd(IntPtr taskBarWnd)
	{
		Native.GetWindowThreadProcessId(taskBarWnd, out var lpdwProcessId);
		foreach (ProcessThread thread in Process.GetProcessById((int)lpdwProcessId).Threads)
		{
			Native.EnumThreadWindows(thread.Id, MyEnumThreadWindowsProc, IntPtr.Zero);
		}
		return vistaStartMenuWnd;
	}

	private static bool MyEnumThreadWindowsProc(IntPtr hWnd, IntPtr lParam)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		if (Native.GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity) > 0 && stringBuilder.ToString() == "Start")
		{
			vistaStartMenuWnd = hWnd;
			return false;
		}
		return true;
	}
}
