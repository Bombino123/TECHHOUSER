using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GMap.NET.MapProviders;

namespace GMap.NET.Internals;

internal class TileHttpHost
{
	private volatile bool _listen;

	private TcpListener _server;

	private int _port;

	private readonly byte[] _responseHeaderBytes;

	public TileHttpHost()
	{
		string s = "HTTP/1.0 200 OK\r\nContent-Type: image\r\nConnection: close\r\n\r\n";
		_responseHeaderBytes = Encoding.ASCII.GetBytes(s);
	}

	public void Stop()
	{
		if (_listen)
		{
			_listen = false;
			if (_server != null)
			{
				_server.Stop();
			}
		}
	}

	public void Start(int port)
	{
		if (_server == null)
		{
			_port = port;
			_server = new TcpListener(IPAddress.Any, port);
		}
		else if (_port != port)
		{
			Stop();
			_port = port;
			_server = null;
			_server = new TcpListener(IPAddress.Any, port);
		}
		else if (_listen)
		{
			return;
		}
		_server.Start();
		_listen = true;
		Thread thread = new Thread((ThreadStart)delegate
		{
			while (_listen)
			{
				try
				{
					if (!_server.Pending())
					{
						Thread.Sleep(111);
					}
					else
					{
						ThreadPool.QueueUserWorkItem(ProcessRequest, _server.AcceptTcpClient());
					}
				}
				catch (Exception)
				{
				}
			}
		});
		thread.Name = "TileHost";
		thread.IsBackground = true;
		thread.Start();
	}

	private void ProcessRequest(object p)
	{
		try
		{
			using TcpClient tcpClient = p as TcpClient;
			using (NetworkStream networkStream = tcpClient.GetStream())
			{
				using StreamReader streamReader = new StreamReader(networkStream, Encoding.UTF8);
				string text = streamReader.ReadLine();
				if (!string.IsNullOrEmpty(text) && text.StartsWith("GET"))
				{
					string[] array = text.Split(new char[1] { ' ' });
					if (array.Length >= 2)
					{
						string[] array2 = array[1].Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
						if (array2.Length == 4)
						{
							int dbId = int.Parse(array2[0]);
							int zoom = int.Parse(array2[1]);
							int num = int.Parse(array2[2]);
							int num2 = int.Parse(array2[3]);
							GMapProvider gMapProvider = GMapProviders.TryGetProvider(dbId);
							if (gMapProvider != null)
							{
								Exception result;
								PureImage imageFrom = GMaps.Instance.GetImageFrom(gMapProvider, new GPoint(num, num2), zoom, out result);
								if (imageFrom != null)
								{
									using (imageFrom)
									{
										networkStream.Write(_responseHeaderBytes, 0, _responseHeaderBytes.Length);
										imageFrom.Data.WriteTo(networkStream);
									}
								}
							}
						}
					}
				}
			}
			tcpClient.Close();
		}
		catch (Exception)
		{
		}
	}
}
