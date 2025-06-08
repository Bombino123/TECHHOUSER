using System;
using System.Text;

namespace Plugin.Handler;

public class HandleDesktop
{
	public enum DesktopWindow
	{
		ProgMan,
		SHELLDLL_DefViewParent,
		SHELLDLL_DefView,
		SysListView32
	}

	public void Show()
	{
		try
		{
			SetDesktopVisibility(visible: true);
		}
		catch
		{
		}
	}

	public void Hide()
	{
		try
		{
			SetDesktopVisibility(visible: false);
		}
		catch
		{
		}
	}

	public static void SetDesktopVisibility(bool visible)
	{
		Native.ShowWindow(GetDesktopWindow(DesktopWindow.ProgMan), visible ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
	}

	public static IntPtr GetDesktopWindow(DesktopWindow desktopWindow)
	{
		IntPtr shelldllDefViewParent;
		IntPtr intPtr = (shelldllDefViewParent = Native.GetShellWindow());
		IntPtr shelldllDefView = Native.FindWindowEx(intPtr, IntPtr.Zero, "SHELLDLL_DefView", null);
		IntPtr sysListView32 = Native.FindWindowEx(shelldllDefView, IntPtr.Zero, "SysListView32", "FolderView");
		if (shelldllDefView == IntPtr.Zero)
		{
			Native.EnumWindows(delegate(IntPtr hwnd, IntPtr lParam)
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				if (Native.GetClassName(hwnd, stringBuilder, 256) > 0 && stringBuilder.ToString() == "WorkerW")
				{
					IntPtr intPtr2 = Native.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
					if (intPtr2 != IntPtr.Zero)
					{
						shelldllDefViewParent = hwnd;
						shelldllDefView = intPtr2;
						sysListView32 = Native.FindWindowEx(intPtr2, IntPtr.Zero, "SysListView32", "FolderView");
						return false;
					}
				}
				return true;
			}, IntPtr.Zero);
		}
		return desktopWindow switch
		{
			DesktopWindow.ProgMan => intPtr, 
			DesktopWindow.SHELLDLL_DefViewParent => shelldllDefViewParent, 
			DesktopWindow.SHELLDLL_DefView => shelldllDefView, 
			DesktopWindow.SysListView32 => sysListView32, 
			_ => IntPtr.Zero, 
		};
	}
}
