using System;

namespace AntdUI;

internal static class Windows7Taskbar
{
	private static ITaskbarList3? _taskbarList;

	internal static ITaskbarList3 TaskbarList
	{
		get
		{
			if (_taskbarList == null)
			{
				_taskbarList = (ITaskbarList3)new CTaskbarList();
				_taskbarList.HrInit();
			}
			return _taskbarList;
		}
	}

	public static void SetProgressState(IntPtr hwnd, ThumbnailProgressState state)
	{
		TaskbarList.SetProgressState(hwnd, state);
	}

	public static void SetProgressValue(IntPtr hwnd, ulong current, ulong maximum)
	{
		TaskbarList.SetProgressValue(hwnd, current, maximum);
	}

	public static void SetProgressValue(IntPtr hwnd, ulong current)
	{
		TaskbarList.SetProgressValue(hwnd, current, 100uL);
	}
}
