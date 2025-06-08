using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Org.Mentalis.Network.ProxySocket;

namespace GMap.NET.Internals;

internal class SocksHttpWebRequest : WebRequest
{
	private WebHeaderCollection _requestHeaders;

	private string _method;

	private SocksHttpWebResponse _response;

	private string _requestMessage;

	private byte[] _requestContentBuffer;

	private static readonly StringCollection ValidHttpVerbs = new StringCollection { "GET", "HEAD", "POST", "PUT", "DELETE", "TRACE", "OPTIONS" };

	public override Uri RequestUri { get; }

	public override IWebProxy Proxy { get; set; }

	public override WebHeaderCollection Headers
	{
		get
		{
			if (_requestHeaders == null)
			{
				_requestHeaders = new WebHeaderCollection();
			}
			return _requestHeaders;
		}
		set
		{
			if (RequestSubmitted)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			_requestHeaders = value;
		}
	}

	public bool RequestSubmitted { get; private set; }

	public override string Method
	{
		get
		{
			return _method ?? "GET";
		}
		set
		{
			if (ValidHttpVerbs.Contains(value))
			{
				_method = value;
				return;
			}
			throw new ArgumentOutOfRangeException("value", $"'{value}' is not a known HTTP verb.");
		}
	}

	public override long ContentLength { get; set; }

	public override string ContentType { get; set; }

	public string RequestMessage
	{
		get
		{
			if (string.IsNullOrEmpty(_requestMessage))
			{
				_requestMessage = BuildHttpRequestMessage();
			}
			return _requestMessage;
		}
	}

	private SocksHttpWebRequest(Uri requestUri)
	{
		RequestUri = requestUri;
	}

	public override WebResponse GetResponse()
	{
		if (Proxy == null)
		{
			throw new InvalidOperationException("Proxy property cannot be null.");
		}
		if (string.IsNullOrEmpty(Method))
		{
			throw new InvalidOperationException("Method has not been set.");
		}
		if (RequestSubmitted)
		{
			return _response;
		}
		_response = InternalGetResponse();
		RequestSubmitted = true;
		return _response;
	}

	public override Stream GetRequestStream()
	{
		if (RequestSubmitted)
		{
			throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
		}
		if (_requestContentBuffer == null)
		{
			_requestContentBuffer = new byte[ContentLength];
		}
		else if (ContentLength == 0L)
		{
			_requestContentBuffer = new byte[int.MaxValue];
		}
		else if (_requestContentBuffer.Length != ContentLength)
		{
			Array.Resize(ref _requestContentBuffer, (int)ContentLength);
		}
		return new MemoryStream(_requestContentBuffer);
	}

	public new static WebRequest Create(string requestUri)
	{
		return new SocksHttpWebRequest(new Uri(requestUri));
	}

	public new static WebRequest Create(Uri requestUri)
	{
		return new SocksHttpWebRequest(requestUri);
	}

	private string BuildHttpRequestMessage()
	{
		if (RequestSubmitted)
		{
			throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0} {1} HTTP/1.0\r\nHost: {2}\r\n", Method, RequestUri.PathAndQuery, RequestUri.Host);
		foreach (object key in Headers.Keys)
		{
			stringBuilder.AppendFormat("{0}: {1}\r\n", key, Headers[key.ToString()]);
		}
		if (!string.IsNullOrEmpty(ContentType))
		{
			stringBuilder.AppendFormat("Content-Type: {0}\r\n", ContentType);
		}
		if (ContentLength > 0)
		{
			stringBuilder.AppendFormat("Content-Length: {0}\r\n", ContentLength);
		}
		stringBuilder.Append("\r\n");
		if (_requestContentBuffer != null && _requestContentBuffer.Length != 0)
		{
			using MemoryStream stream = new MemoryStream(_requestContentBuffer, writable: false);
			using StreamReader streamReader = new StreamReader(stream);
			stringBuilder.Append(streamReader.ReadToEnd());
		}
		return stringBuilder.ToString();
	}

	private SocksHttpWebResponse InternalGetResponse()
	{
		MemoryStream memoryStream = null;
		string text = string.Empty;
		using (ProxySocket proxySocket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
		{
			Uri proxy = Proxy.GetProxy(RequestUri);
			IPAddress proxyIpAddress = GetProxyIpAddress(proxy);
			proxySocket.ProxyEndPoint = new IPEndPoint(proxyIpAddress, proxy.Port);
			proxySocket.ProxyType = ProxyTypes.Socks5;
			proxySocket.Connect(RequestUri.Host, 80);
			proxySocket.Send(Encoding.UTF8.GetBytes(RequestMessage));
			byte[] array = new byte[4096];
			bool flag = false;
			int num;
			while ((num = proxySocket.Receive(array)) > 0)
			{
				if (!flag)
				{
					string @string = Encoding.UTF8.GetString(array, 0, (num > 1024) ? 1024 : num);
					int num2 = @string.IndexOf("\r\n\r\n");
					if (num2 > 0)
					{
						@string = @string.Substring(0, num2);
						text += @string;
						flag = true;
						int num3 = Encoding.UTF8.GetByteCount(@string) + 4;
						if (num3 < num)
						{
							memoryStream = new MemoryStream();
							memoryStream.Write(array, num3, num - num3);
						}
					}
					else
					{
						text += @string;
					}
				}
				else
				{
					if (memoryStream == null)
					{
						memoryStream = new MemoryStream();
					}
					memoryStream.Write(array, 0, num);
				}
			}
			if (memoryStream != null)
			{
				memoryStream.Position = 0L;
			}
		}
		return new SocksHttpWebResponse(memoryStream, text);
	}

	private static IPAddress GetProxyIpAddress(Uri proxyUri)
	{
		if (!IPAddress.TryParse(proxyUri.Host, out IPAddress address))
		{
			try
			{
				return Dns.GetHostEntry(proxyUri.Host).AddressList[0];
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException($"Unable to resolve proxy hostname '{proxyUri.Host}' to a valid IP address.", innerException);
			}
		}
		return address;
	}
}
