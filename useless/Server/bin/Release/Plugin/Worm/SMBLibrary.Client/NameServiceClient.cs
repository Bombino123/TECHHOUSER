using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using SMBLibrary.NetBios;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class NameServiceClient
{
	public static readonly int NetBiosNameServicePort = 137;

	private IPAddress m_serverAddress;

	public NameServiceClient(IPAddress serverAddress)
	{
		m_serverAddress = serverAddress;
	}

	public string GetServerName()
	{
		NodeStatusRequest nodeStatusRequest = new NodeStatusRequest();
		nodeStatusRequest.Header.QDCount = 1;
		nodeStatusRequest.Question.Name = "*".PadRight(16, '\0');
		foreach (KeyValuePair<string, NameFlags> name in SendNodeStatusRequest(nodeStatusRequest).Names)
		{
			if (NetBiosUtils.GetSuffixFromMSNetBiosName(name.Key) == NetBiosSuffix.FileServiceService)
			{
				return name.Key;
			}
		}
		return null;
	}

	private NodeStatusResponse SendNodeStatusRequest(NodeStatusRequest request)
	{
		UdpClient udpClient = new UdpClient();
		IPEndPoint remoteEP = new IPEndPoint(m_serverAddress, NetBiosNameServicePort);
		udpClient.Connect(remoteEP);
		byte[] bytes = request.GetBytes();
		udpClient.Send(bytes, bytes.Length);
		return new NodeStatusResponse(udpClient.Receive(ref remoteEP), 0);
	}
}
