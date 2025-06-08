using System.Net.Sockets;
using System.Runtime.InteropServices;
using SMBLibrary.NetBios;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class ConnectionState
{
	private Socket m_clientSocket;

	private NBTConnectionReceiveBuffer m_receiveBuffer;

	public Socket ClientSocket => m_clientSocket;

	public NBTConnectionReceiveBuffer ReceiveBuffer => m_receiveBuffer;

	public ConnectionState(Socket clientSocket)
	{
		m_clientSocket = clientSocket;
		m_receiveBuffer = new NBTConnectionReceiveBuffer();
	}
}
