using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class IPAddressHelper
{
	public static IPAddress SelectAddressPreferIPv4(IPAddress[] hostAddresses)
	{
		foreach (IPAddress iPAddress in hostAddresses)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				return iPAddress;
			}
		}
		return hostAddresses[0];
	}
}
