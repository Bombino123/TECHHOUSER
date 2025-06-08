using System;
using System.Net;
using System.Net.Sockets;

namespace Org.Mentalis.Network.ProxySocket;

internal abstract class SocksHandler
{
	private Socket _server;

	private string _username;

	protected HandShakeComplete ProtocolComplete;

	protected Socket Server
	{
		get
		{
			return _server;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			_server = value;
		}
	}

	protected string Username
	{
		get
		{
			return _username;
		}
		set
		{
			_username = value ?? throw new ArgumentNullException();
		}
	}

	protected IAsyncProxyResult AsyncResult { get; set; }

	protected byte[] Buffer { get; set; }

	protected int Received { get; set; }

	public SocksHandler(Socket server, string user)
	{
		Server = server;
		Username = user;
	}

	protected byte[] PortToBytes(int port)
	{
		return new byte[2]
		{
			(byte)(port / 256),
			(byte)(port % 256)
		};
	}

	protected byte[] ReadBytes(int count)
	{
		if (count <= 0)
		{
			throw new ArgumentException();
		}
		byte[] array = new byte[count];
		for (int i = 0; i != count; i += Server.Receive(array, i, count - i, SocketFlags.None))
		{
		}
		return array;
	}

	public abstract void Negotiate(string host, int port);

	public abstract void Negotiate(IPEndPoint remoteEP);

	public abstract IAsyncProxyResult BeginNegotiate(IPEndPoint remoteEP, HandShakeComplete callback, IPEndPoint proxyEndPoint);

	public abstract IAsyncProxyResult BeginNegotiate(string host, int port, HandShakeComplete callback, IPEndPoint proxyEndPoint);
}
