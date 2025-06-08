using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Leb128;

namespace Plugin.Helper;

public class Client
{
	public static Socket socket;

	public static SslStream SslClient;

	public static byte[] ClientBuffer;

	public static bool ClientBufferRecevied;

	public static int HeaderSize;

	public static int Offset;

	public static object SendSync;

	public static bool itsConnect;

	public static KeepPing keepPing;

	public static void Connect(string ip, string port)
	{
		try
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.ReceiveBufferSize = 10240;
			socket.SendBufferSize = 10240;
			socket.Connect(ip, Convert.ToInt32(port));
			if (socket.Connected)
			{
				itsConnect = true;
				SendSync = new object();
				SslClient = new SslStream(new NetworkStream(socket, ownsSocket: true), leaveInnerStreamOpen: false, ValidateServerCertificate);
				SslClient.AuthenticateAsClient(socket.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], null, SslProtocols.Tls, checkCertificateRevocation: false);
				Offset = 0;
				HeaderSize = 4;
				ClientBuffer = new byte[HeaderSize];
				ClientBufferRecevied = false;
				SslClient.BeginRead(ClientBuffer, Offset, HeaderSize, ReadData, null);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			Disconnect();
		}
	}

	private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		return Plugin.X509Certificate2.Equals(certificate);
	}

	public static void Disconnect()
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
		}
	}

	public static void ReadData(IAsyncResult ar)
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
					Packet.Read(ClientBuffer);
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

	public static void Error(string exp)
	{
		Send(LEB128.Write(new object[2] { "Error", exp }));
	}

	public static void Send(byte[] Data)
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
				if (Data.Length > 51200)
				{
					using (MemoryStream memoryStream = new MemoryStream(Data))
					{
						memoryStream.Position = 0L;
						byte[] array = new byte[51200];
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
