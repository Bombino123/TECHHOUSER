using System;

namespace Stub.Helper;

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
			try
			{
				SuperCore.RunWithTokenOf("winlogon.exe", OfActiveSessionOnly: true, Name, " /WithTokenOf:TrustedInstaller.exe");
				Environment.Exit(0);
			}
			catch
			{
			}
		}
	}
}
