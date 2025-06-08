using System;
using System.Net.Sockets;
using System.Text;

namespace Org.Mentalis.Network.ProxySocket.Authentication;

internal sealed class AuthUserPass : AuthMethod
{
	private string _username;

	private string _password;

	private string Username
	{
		get
		{
			return _username;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			_username = value;
		}
	}

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

	public AuthUserPass(Socket server, string user, string pass)
		: base(server)
	{
		Username = user;
		Password = pass;
	}

	private byte[] GetAuthenticationBytes()
	{
		byte[] array = new byte[3 + Username.Length + Password.Length];
		array[0] = 1;
		array[1] = (byte)Username.Length;
		Array.Copy(Encoding.ASCII.GetBytes(Username), 0, array, 2, Username.Length);
		array[Username.Length + 2] = (byte)Password.Length;
		Array.Copy(Encoding.ASCII.GetBytes(Password), 0, array, Username.Length + 3, Password.Length);
		return array;
	}

	public override void Authenticate()
	{
		base.Server.Send(GetAuthenticationBytes());
		byte[] array = new byte[2];
		for (int i = 0; i != 2; i += base.Server.Receive(array, i, 2 - i, SocketFlags.None))
		{
		}
		if (array[1] != 0)
		{
			base.Server.Close();
			throw new ProxyException("Username/password combination rejected.");
		}
	}

	public override void BeginAuthenticate(HandShakeComplete callback)
	{
		CallBack = callback;
		base.Server.BeginSend(GetAuthenticationBytes(), 0, 3 + Username.Length + Password.Length, SocketFlags.None, OnSent, base.Server);
	}

	private void OnSent(IAsyncResult ar)
	{
		try
		{
			base.Server.EndSend(ar);
			base.Buffer = new byte[2];
			base.Server.BeginReceive(base.Buffer, 0, 2, SocketFlags.None, OnReceive, base.Server);
		}
		catch (Exception error)
		{
			CallBack(error);
		}
	}

	private void OnReceive(IAsyncResult ar)
	{
		try
		{
			base.Received += base.Server.EndReceive(ar);
			if (base.Received == base.Buffer.Length)
			{
				if (base.Buffer[1] != 0)
				{
					throw new ProxyException("Username/password combination not accepted.");
				}
				CallBack(null);
			}
			else
			{
				base.Server.BeginReceive(base.Buffer, base.Received, base.Buffer.Length - base.Received, SocketFlags.None, OnReceive, base.Server);
			}
		}
		catch (Exception error)
		{
			CallBack(error);
		}
	}
}
