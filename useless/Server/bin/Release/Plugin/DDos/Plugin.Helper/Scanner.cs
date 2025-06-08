using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Plugin.Helper;

internal class Scanner
{
	public enum porttype
	{
		TCP,
		HTTP,
		HTTPS
	}

	public class Port
	{
		public porttype porttype;

		public int port;
	}

	public static List<Port> OpenPorts(string url)
	{
		url = url.Replace("https://", "");
		url = url.Replace("http://", "");
		List<Port> ports = new List<Port>();
		Parallel.ForEach(Common.ports, delegate(int port)
		{
			if (TcpCheck(url, port))
			{
				ports.Add(new Port
				{
					porttype = porttype.TCP,
					port = port
				});
				if (HttpCheck(url, port))
				{
					ports.Add(new Port
					{
						porttype = porttype.HTTP,
						port = port
					});
				}
				if (HttpsCheck(url, port))
				{
					ports.Add(new Port
					{
						porttype = porttype.HTTPS,
						port = port
					});
				}
			}
		});
		return ports;
	}

	public static bool TcpCheck(string url, int port)
	{
		try
		{
			new TcpClient().Connect(url, port);
			return true;
		}
		catch
		{
		}
		return false;
	}

	public static bool HttpCheck(string url, int port)
	{
		try
		{
			new WebClient().DownloadString("http://" + url + ":" + port);
			return true;
		}
		catch
		{
		}
		return false;
	}

	public static bool HttpsCheck(string url, int port)
	{
		try
		{
			try
			{
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
				ServicePointManager.DefaultConnectionLimit = 9999;
			}
			catch (Exception)
			{
			}
			new WebClient().DownloadString("https://" + url + ":" + port);
			return true;
		}
		catch
		{
		}
		return false;
	}
}
