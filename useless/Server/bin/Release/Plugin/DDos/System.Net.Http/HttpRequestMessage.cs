using System.Collections.Generic;
using System.Net.Http.Headers;

namespace System.Net.Http;

public class HttpRequestMessage : IDisposable
{
	public extern Version Version { get; set; }

	public extern HttpContent Content { get; set; }

	public extern HttpMethod Method { get; set; }

	public extern Uri RequestUri { get; set; }

	public extern HttpRequestHeaders Headers { get; }

	public extern IDictionary<string, object> Properties { get; }

	public extern HttpRequestMessage();

	public extern HttpRequestMessage(HttpMethod method, Uri requestUri);

	public extern HttpRequestMessage(HttpMethod method, string requestUri);

	public override extern string ToString();

	internal extern bool MarkAsSent();

	protected virtual extern void Dispose(bool disposing);

	public extern void Dispose();
}
