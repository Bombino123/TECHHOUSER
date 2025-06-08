using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Org.Mentalis.Network.ProxySocket.Authentication;

namespace Org.Mentalis.Network.ProxySocket;

internal sealed class Socks5Handler : SocksHandler
{
	private string _password;

	private string Password
	{
		get
		{
			return _password;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			_password = value;
		}
	}

	private byte[] HandShake { get; set; }

	public Socks5Handler(Socket server)
		: this(server, "")
	{
	}

	public Socks5Handler(Socket server, string user)
		: this(server, user, "")
	{
	}

	public Socks5Handler(Socket server, string user, string pass)
		: base(server, user)
	{
		Password = pass;
	}

	private void Authenticate()
	{
		base.Server.Send(new byte[4] { 5, 2, 0, 2 });
		byte[] array = ReadBytes(2);
		if (array[1] == byte.MaxValue)
		{
			throw new ProxyException("No authentication method accepted.");
		}
		(array[1] switch
		{
			0 => new AuthNone(base.Server), 
			2 => new AuthUserPass(base.Server, base.Username, Password), 
			_ => throw new ProtocolViolationException(), 
		}).Authenticate();
	}

	private byte[] GetHostPortBytes(string host, int port)
	{
		if (host == null)
		{
			throw new ArgumentNullException();
		}
		if (port <= 0 || port > 65535 || host.Length > 255)
		{
			throw new ArgumentException();
		}
		byte[] array = new byte[7 + host.Length];
		array[0] = 5;
		array[1] = 1;
		array[2] = 0;
		array[3] = 3;
		array[4] = (byte)host.Length;
		Array.Copy(Encoding.ASCII.GetBytes(host), 0, array, 5, host.Length);
		Array.Copy(PortToBytes(port), 0, array, host.Length + 5, 2);
		return array;
	}

	private byte[] GetEndPointBytes(IPEndPoint remoteEP)
	{
		if (remoteEP == null)
		{
			throw new ArgumentNullException();
		}
		byte[] array = new byte[10] { 5, 1, 0, 1, 0, 0, 0, 0, 0, 0 };
		Array.Copy(remoteEP.Address.GetAddressBytes(), 0, array, 4, 4);
		Array.Copy(PortToBytes(remoteEP.Port), 0, array, 8, 2);
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
		Authenticate();
		base.Server.Send(connect);
		byte[] array = ReadBytes(4);
		if (array[1] != 0)
		{
			base.Server.Close();
			throw new ProxyException(array[1]);
		}
		switch (array[3])
		{
		case 1:
			array = ReadBytes(6);
			break;
		case 3:
			array = ReadBytes(1);
			array = ReadBytes(array[0] + 2);
			break;
		case 4:
			array = ReadBytes(18);
			break;
		default:
			base.Server.Close();
			throw new ProtocolViolationException();
		}
	}

	public override IAsyncProxyResult BeginNegotiate(string host, int port, HandShakeComplete callback, IPEndPoint proxyEndPoint)
	{
		ProtocolComplete = callback;
		HandShake = GetHostPortBytes(host, port);
		base.Server.BeginConnect(proxyEndPoint, OnConnect, base.Server);
		base.AsyncResult = new IAsyncProxyResult();
		return base.AsyncResult;
	}

	public override IAsyncProxyResult BeginNegotiate(IPEndPoint remoteEP, HandShakeComplete callback, IPEndPoint proxyEndPoint)
	{
		ProtocolComplete = callback;
		HandShake = GetEndPointBytes(remoteEP);
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
			base.Server.BeginSend(new byte[4] { 5, 2, 0, 2 }, 0, 4, SocketFlags.None, OnAuthSent, base.Server);
		}
		catch (Exception error2)
		{
			ProtocolComplete(error2);
		}
	}

	private void OnAuthSent(IAsyncResult ar)
	{
		try
		{
			base.Server.EndSend(ar);
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
			return;
		}
		try
		{
			base.Buffer = new byte[1024];
			base.Received = 0;
			base.Server.BeginReceive(base.Buffer, 0, base.Buffer.Length, SocketFlags.None, OnAuthReceive, base.Server);
		}
		catch (Exception error2)
		{
			ProtocolComplete(error2);
		}
	}

	private void OnAuthReceive(IAsyncResult ar)
	{
		try
		{
			base.Received += base.Server.EndReceive(ar);
			if (base.Received <= 0)
			{
				throw new SocketException();
			}
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
			return;
		}
		try
		{
			if (base.Received < 2)
			{
				base.Server.BeginReceive(base.Buffer, base.Received, base.Buffer.Length - base.Received, SocketFlags.None, OnAuthReceive, base.Server);
				return;
			}
			AuthMethod authMethod;
			switch (base.Buffer[1])
			{
			case 0:
				authMethod = new AuthNone(base.Server);
				break;
			case 2:
				authMethod = new AuthUserPass(base.Server, base.Username, Password);
				break;
			default:
				ProtocolComplete(new SocketException());
				return;
			}
			authMethod.BeginAuthenticate(OnAuthenticated);
		}
		catch (Exception error2)
		{
			ProtocolComplete(error2);
		}
	}

	private void OnAuthenticated(Exception e)
	{
		if (e != null)
		{
			ProtocolComplete(e);
			return;
		}
		try
		{
			base.Server.BeginSend(HandShake, 0, HandShake.Length, SocketFlags.None, OnSent, base.Server);
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
		}
	}

	private void OnSent(IAsyncResult ar)
	{
		try
		{
			base.Server.EndSend(ar);
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
			return;
		}
		try
		{
			base.Buffer = new byte[5];
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
			base.Received += base.Server.EndReceive(ar);
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
			return;
		}
		try
		{
			if (base.Received == base.Buffer.Length)
			{
				ProcessReply(base.Buffer);
			}
			else
			{
				base.Server.BeginReceive(base.Buffer, base.Received, base.Buffer.Length - base.Received, SocketFlags.None, OnReceive, base.Server);
			}
		}
		catch (Exception error2)
		{
			ProtocolComplete(error2);
		}
	}

	private void ProcessReply(byte[] buffer)
	{
		switch (buffer[3])
		{
		case 1:
			base.Buffer = new byte[5];
			break;
		case 3:
			base.Buffer = new byte[buffer[4] + 2];
			break;
		case 4:
			buffer = new byte[17];
			break;
		default:
			throw new ProtocolViolationException();
		}
		base.Received = 0;
		base.Server.BeginReceive(base.Buffer, 0, base.Buffer.Length, SocketFlags.None, OnReadLast, base.Server);
	}

	private void OnReadLast(IAsyncResult ar)
	{
		try
		{
			base.Received += base.Server.EndReceive(ar);
		}
		catch (Exception error)
		{
			ProtocolComplete(error);
			return;
		}
		try
		{
			if (base.Received == base.Buffer.Length)
			{
				ProtocolComplete(null);
			}
			else
			{
				base.Server.BeginReceive(base.Buffer, base.Received, base.Buffer.Length - base.Received, SocketFlags.None, OnReadLast, base.Server);
			}
		}
		catch (Exception error2)
		{
			ProtocolComplete(error2);
		}
	}
}
