using System;
using System.Net;
using System.Net.Sockets;

namespace Org.Mentalis.Network.ProxySocket;

internal class ProxySocket : Socket
{
	private string _proxyUser;

	private string _proxyPass;

	private AsyncCallback _callBack;

	public IPEndPoint ProxyEndPoint { get; set; }

	public ProxyTypes ProxyType { get; set; }

	private object State { get; set; }

	public string ProxyUser
	{
		get
		{
			return _proxyUser;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			_proxyUser = value;
		}
	}

	public string ProxyPass
	{
		get
		{
			return _proxyPass;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			_proxyPass = value;
		}
	}

	private IAsyncProxyResult AsyncResult { get; set; }

	private Exception ToThrow { get; set; }

	private int RemotePort { get; set; }

	public ProxySocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		: this(addressFamily, socketType, protocolType, "")
	{
	}

	public ProxySocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string proxyUsername)
		: this(addressFamily, socketType, protocolType, proxyUsername, "")
	{
	}

	public ProxySocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string proxyUsername, string proxyPassword)
		: base(addressFamily, socketType, protocolType)
	{
		ProxyUser = proxyUsername;
		ProxyPass = proxyPassword;
		ToThrow = new InvalidOperationException();
	}

	public new void Connect(EndPoint remoteEP)
	{
		if (remoteEP == null)
		{
			throw new ArgumentNullException("<remoteEP> cannot be null.");
		}
		if (base.ProtocolType != ProtocolType.Tcp || ProxyType == ProxyTypes.None || ProxyEndPoint == null)
		{
			base.Connect(remoteEP);
			return;
		}
		base.Connect((EndPoint)ProxyEndPoint);
		if (ProxyType == ProxyTypes.Socks4)
		{
			new Socks4Handler(this, ProxyUser).Negotiate((IPEndPoint)remoteEP);
		}
		else if (ProxyType == ProxyTypes.Socks5)
		{
			new Socks5Handler(this, ProxyUser, ProxyPass).Negotiate((IPEndPoint)remoteEP);
		}
	}

	public new void Connect(string host, int port)
	{
		if (host == null)
		{
			throw new ArgumentNullException("<host> cannot be null.");
		}
		if (port <= 0 || port > 65535)
		{
			throw new ArgumentException("Invalid port.");
		}
		if (base.ProtocolType != ProtocolType.Tcp || ProxyType == ProxyTypes.None || ProxyEndPoint == null)
		{
			base.Connect((EndPoint)new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port));
			return;
		}
		base.Connect((EndPoint)ProxyEndPoint);
		if (ProxyType == ProxyTypes.Socks4)
		{
			new Socks4Handler(this, ProxyUser).Negotiate(host, port);
		}
		else if (ProxyType == ProxyTypes.Socks5)
		{
			new Socks5Handler(this, ProxyUser, ProxyPass).Negotiate(host, port);
		}
	}

	public new IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
	{
		if (remoteEP == null || callback == null)
		{
			throw new ArgumentNullException();
		}
		if (base.ProtocolType != ProtocolType.Tcp || ProxyType == ProxyTypes.None || ProxyEndPoint == null)
		{
			return base.BeginConnect(remoteEP, callback, state);
		}
		_callBack = callback;
		if (ProxyType == ProxyTypes.Socks4)
		{
			AsyncResult = new Socks4Handler(this, ProxyUser).BeginNegotiate((IPEndPoint)remoteEP, OnHandShakeComplete, ProxyEndPoint);
			return AsyncResult;
		}
		if (ProxyType == ProxyTypes.Socks5)
		{
			AsyncResult = new Socks5Handler(this, ProxyUser, ProxyPass).BeginNegotiate((IPEndPoint)remoteEP, OnHandShakeComplete, ProxyEndPoint);
			return AsyncResult;
		}
		return null;
	}

	public new IAsyncResult BeginConnect(string host, int port, AsyncCallback callback, object state)
	{
		if (host == null || callback == null)
		{
			throw new ArgumentNullException();
		}
		if (port <= 0 || port > 65535)
		{
			throw new ArgumentException();
		}
		_callBack = callback;
		if (base.ProtocolType != ProtocolType.Tcp || ProxyType == ProxyTypes.None || ProxyEndPoint == null)
		{
			RemotePort = port;
			AsyncResult = BeginDns(host, OnHandShakeComplete);
			return AsyncResult;
		}
		if (ProxyType == ProxyTypes.Socks4)
		{
			AsyncResult = new Socks4Handler(this, ProxyUser).BeginNegotiate(host, port, OnHandShakeComplete, ProxyEndPoint);
			return AsyncResult;
		}
		if (ProxyType == ProxyTypes.Socks5)
		{
			AsyncResult = new Socks5Handler(this, ProxyUser, ProxyPass).BeginNegotiate(host, port, OnHandShakeComplete, ProxyEndPoint);
			return AsyncResult;
		}
		return null;
	}

	public new void EndConnect(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException();
		}
		if (!asyncResult.IsCompleted)
		{
			throw new ArgumentException();
		}
		if (ToThrow != null)
		{
			throw ToThrow;
		}
	}

	internal IAsyncProxyResult BeginDns(string host, HandShakeComplete callback)
	{
		try
		{
			Dns.BeginGetHostEntry(host, OnResolved, this);
			return new IAsyncProxyResult();
		}
		catch
		{
			throw new SocketException();
		}
	}

	private void OnResolved(IAsyncResult asyncResult)
	{
		try
		{
			IPHostEntry iPHostEntry = Dns.EndGetHostEntry(asyncResult);
			base.BeginConnect((EndPoint)new IPEndPoint(iPHostEntry.AddressList[0], RemotePort), (AsyncCallback?)OnConnect, State);
		}
		catch (Exception error)
		{
			OnHandShakeComplete(error);
		}
	}

	private void OnConnect(IAsyncResult asyncResult)
	{
		try
		{
			base.EndConnect(asyncResult);
			OnHandShakeComplete(null);
		}
		catch (Exception error)
		{
			OnHandShakeComplete(error);
		}
	}

	private void OnHandShakeComplete(Exception error)
	{
		if (error != null)
		{
			Close();
		}
		ToThrow = error;
		AsyncResult.Reset();
		if (_callBack != null)
		{
			_callBack(AsyncResult);
		}
	}
}
