using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Org.Mentalis.Network.ProxySocket;

internal sealed class Socks4Handler : SocksHandler
{
	public Socks4Handler(Socket server, string user)
		: base(server, user)
	{
	}

	private byte[] GetHostPortBytes(string host, int port)
	{
		if (host == null)
		{
			throw new ArgumentNullException();
		}
		if (port <= 0 || port > 65535)
		{
			throw new ArgumentException();
		}
		byte[] array = new byte[10 + base.Username.Length + host.Length];
		array[0] = 4;
		array[1] = 1;
		Array.Copy(PortToBytes(port), 0, array, 2, 2);
		array[4] = (array[5] = (array[6] = 0));
		array[7] = 1;
		Array.Copy(Encoding.ASCII.GetBytes(base.Username), 0, array, 8, base.Username.Length);
		array[8 + base.Username.Length] = 0;
		Array.Copy(Encoding.ASCII.GetBytes(host), 0, array, 9 + base.Username.Length, host.Length);
		array[9 + base.Username.Length + host.Length] = 0;
		return array;
	}

	private byte[] GetEndPointBytes(IPEndPoint remoteEP)
	{
		if (remoteEP == null)
		{
			throw new ArgumentNullException();
		}
		byte[] array = new byte[9 + base.Username.Length];
		array[0] = 4;
		array[1] = 1;
		Array.Copy(PortToBytes(remoteEP.Port), 0, array, 2, 2);
		Array.Copy(remoteEP.Address.GetAddressBytes(), 0, array, 4, 4);
		Array.Copy(Encoding.ASCII.GetBytes(base.Username), 0, array, 8, base.Username.Length);
		array[8 + base.Username.Length] = 0;
		return array;
	}

	public override void Negotiate(string host, int port)
	{
		Negotiate(GetHostPortBytes(host, port));
	}

	public override void Negotiate(IPEndPoint remoteEP)
	{
		Negotiate(GetEndPointBytes(remoteEP));
	}

	private void Negotiate(byte[] connect)
	{
		if (connect == null)
		{
			throw new ArgumentNullException();
		}
		if (connect.Length < 2)
		{
			throw new ArgumentException();
		}
		base.Server.Send(connect);
		if (ReadBytes(8)[1] != 90)
		{
			base.Server.Close();
			throw new ProxyException("Negotiation failed.");
		}
	}

	public override IAsyncProxyResult BeginNegotiate(string host, int port, HandShakeComplete callback, IPEndPoint proxyEndPoint)
	{
		ProtocolComplete = callback;
		base.Buffer = GetHostPortBytes(host, port);
		base.Server.BeginConnect(proxyEndPoint, OnConnect, base.Server);
		base.AsyncResult = new IAsyncProxyResult();
		return base.AsyncResult;
	}

	public override IAsyncProxyResult BeginNegotiate(IPEndPoint remoteEP, HandShakeComplete callback, IPEndPoint proxyEndPoint)
	{
		ProtocolComplete = callback;
		base.Buffer = GetEndPointBytes(remoteEP);
		base.Server.BeginConnect(proxyEndPoint, OnConnect, base.Server);
		base.AsyncResult = new IAsyncProxyResult();
		return base.AsyncResult;
	}

	private void OnConnect(IAsyncResult ar)
	{
		try
		{
			base.Server.EndConnect(ar);
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
			return;
		}
		try
		{
			base.Server.BeginSend(base.Buffer, 0, base.Buffer.Length, SocketFlags.None, OnSent, base.Server);
		}
		catch (Exception error2)
		{
			ProtocolComplete(error2);
		}
	}

	private void OnSent(IAsyncResult ar)
	{
		try
		{
			if (base.Server.EndSend(ar) < base.Buffer.Length)
			{
				ProtocolComplete(new SocketException());
				return;
			}
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
			return;
		}
		try
		{
			base.Buffer = new byte[8];
			base.Received = 0;
			base.Server.BeginReceive(base.Buffer, 0, base.Buffer.Length, SocketFlags.None, OnReceive, base.Server);
		}
		catch (Exception error2)
		{
			ProtocolComplete(error2);
		}
	}

	private void OnReceive(IAsyncResult ar)
	{
		try
		{
			int num = base.Server.EndReceive(ar);
			if (num <= 0)
			{
				ProtocolComplete(new SocketException());
				return;
			}
			base.Received += num;
			if (base.Received == 8)
			{
				if (base.Buffer[1] == 90)
				{
					ProtocolComplete(null);
					return;
				}
				base.Server.Close();
				ProtocolComplete(new ProxyException("Negotiation failed."));
			}
			else
			{
				base.Server.BeginReceive(base.Buffer, base.Received, base.Buffer.Length - base.Received, SocketFlags.None, OnReceive, base.Server);
			}
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
		}
	}
}
