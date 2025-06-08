using System;
using System.Net;

namespace GMap.NET.MapProviders;

public sealed class EmptyWebProxy : IWebProxy
{
	public static readonly EmptyWebProxy Instance = new EmptyWebProxy();

	public ICredentials Credentials { get; set; }

	public Uri GetProxy(Uri uri)
	{
		return uri;
	}

	public bool IsBypassed(Uri uri)
	{
		return true;
	}
}
