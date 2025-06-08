using System;
using System.IO;
using System.Net;

namespace GMap.NET.Internals;

internal class SocksHttpWebResponse : WebResponse
{
	private WebHeaderCollection _httpResponseHeaders;

	private MemoryStream data;

	public override long ContentLength { get; set; }

	public override string ContentType { get; set; }

	public override WebHeaderCollection Headers
	{
		get
		{
			if (_httpResponseHeaders == null)
			{
				_httpResponseHeaders = new WebHeaderCollection();
			}
			return _httpResponseHeaders;
		}
	}

	public SocksHttpWebResponse(MemoryStream data, string headers)
	{
		this.data = data;
		string[] array = headers.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { ':' });
			Headers.Add(array2[0], array2[1]);
			string text = array2[0];
			if (!(text == "Content-Type"))
			{
				if (text == "Content-Length" && long.TryParse(array2[1], out var result))
				{
					ContentLength = result;
				}
			}
			else
			{
				ContentType = array2[1];
			}
		}
	}

	public override Stream GetResponseStream()
	{
		if (data == null)
		{
			return Stream.Null;
		}
		return data;
	}

	public override void Close()
	{
		if (data != null)
		{
			data.Close();
		}
	}
}
