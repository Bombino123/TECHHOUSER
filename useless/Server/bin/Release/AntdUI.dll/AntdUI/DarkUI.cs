using System;
using System.Runtime.InteropServices;

namespace AntdUI;

public class DarkUI
{
	private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	[DllImport("dwmapi.dll")]
	public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

	[DllImport("dwmapi.dll")]
	public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

	public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
	{
		if (IsWindows10OrGreater(17763))
		{
			int attr = 19;
			if (IsWindows10OrGreater(18985))
			{
				attr = 20;
			}
			int attrValue = (enabled ? 1 : 0);
			return DwmSetWindowAttribute(handle, attr, ref attrValue, 4) == 0;
		}
		return false;
	}

	private static bool IsWindows10OrGreater(int build = -1)
	{
		Version version = OS.Version;
		if (version.Major >= 10)
		{
			return version.Build >= build;
		}
		return false;
	}
}
