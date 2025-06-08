namespace RageStealer.Helper;

internal class Detects
{
	public static string[] BankingServices = new string[7] { "qiwi", "money", "exchange", "bank", "credit", "card", "paypal" };

	public static string[] CryptoServices = new string[24]
	{
		"bitcoin", "monero", "dashcoin", "litecoin", "etherium", "stellarcoin", "btc", "eth", "xmr", "xlm",
		"xrp", "ltc", "bch", "blockchain", "paxful", "investopedia", "buybitcoinworldwide", "cryptocurrency", "crypto", "trade",
		"trading", "wallet", "coinomi", "coinbase"
	};

	public static string[] PornServices = new string[4] { "porn", "sex", "hentai", "chaturbate" };

	public static string[] SocialServices = new string[15]
	{
		"facebook", "vk.com", "ok.ru", "instagram", "whatsapp", "twitter", "gmail", "linkedin", "viber", "skype",
		"reddit", "flickr", "youtube", "pinterest", "tiktok"
	};
}
