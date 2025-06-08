using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Leb128;

namespace Plugin.Helper;

public class ClientTemp
{
	public Socket socket;

	public SslStream SslClient;

	public byte[] ClientBuffer;

	public bool ClientBufferRecevied;

	public long HeaderSize;

	public long Offset;

	public object SendSync;

	public bool itsConnect;

	public KeepPing keepPing;

	public void Connect(string ip, string port)
	{
		try
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.ReceiveBufferSize = 512000;
			socket.SendBufferSize = 512000;
			socket.Connect(ip, Convert.ToInt32(port));
			if (socket.Connected)
			{
				SendSync = new object();
				SslClient = new SslStream(new NetworkStream(socket, ownsSocket: true), leaveInnerStreamOpen: false, ValidateServerCertificate);
				SslClient.AuthenticateAsClient(socket.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], null, SslProtocols.Tls, checkCertificateRevocation: false);
				itsConnect = true;
				Offset = 0L;
				HeaderSize = 4L;
				ClientBuffer = new byte[HeaderSize];
				ClientBufferRecevied = false;
				keepPing = new KeepPing();
				keepPing.Send += delegate
				{
					Send(LEB128.Write(new object[1] { "abc" }));
				};
				SslClient.BeginRead(ClientBuffer, (int)Offset, (int)HeaderSize, ReadData, null);
			}
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
			HeaderSize = 0L;
			Offset = 0L;
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
					if (HeaderSize == 0L)
					{
						HeaderSize = BitConverter.ToInt32(ClientBuffer, 0);
						if (HeaderSize > 0)
						{
							ClientBuffer = new byte[HeaderSize];
							Offset = 0L;
							ClientBufferRecevied = true;
						}
					}
					else if (HeaderSize < 0)
					{
						Disconnect();
						return;
					}
				}
				else if (HeaderSize == 0L)
				{
					Packet.Read(ClientBuffer, this);
					Offset = 0L;
					HeaderSize = 4L;
					ClientBuffer = new byte[HeaderSize];
					ClientBufferRecevied = false;
				}
				else if (HeaderSize < 0)
				{
					Disconnect();
					return;
				}
				SslClient.BeginRead(ClientBuffer, (int)Offset, (int)HeaderSize, ReadData, null);
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
				socket.Poll(-1, SelectMode.SelectWrite);
				SslClient.Write(bytes, 0, bytes.Length);
				if (Data.Length > socket.SendBufferSize)
				{
					using (MemoryStream memoryStream = new MemoryStream(Data))
					{
						memoryStream.Position = 0L;
						byte[] array = new byte[socket.SendBufferSize];
						int count;
						while ((count = memoryStream.Read(array, 0, array.Length)) > 0)
						{
							socket.Poll(-1, SelectMode.SelectWrite);
							SslClient.Write(array, 0, count);
							SslClient.Flush();
						}
						return;
					}
				}
				socket.Poll(-1, SelectMode.SelectWrite);
				SslClient.Write(Data, 0, Data.Length);
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
