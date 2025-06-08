using System;
using System.Net.Sockets;

namespace Org.Mentalis.Network.ProxySocket.Authentication;

internal abstract class AuthMethod
{
	private Socket _server;

	protected HandShakeComplete CallBack;

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

	protected byte[] Buffer { get; set; }

	protected int Received { get; set; }

	public AuthMethod(Socket server)
	{
		Server = server;
	}

	public abstract void Authenticate();

	public abstract void BeginAuthenticate(HandShakeComplete callback);
}
