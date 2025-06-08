using System.Net.Http.Headers;

namespace System.Net.Http;

public class HttpResponseMessage : IDisposable
{
	public extern Version Version { get; set; }

	public extern HttpContent Content { get; set; }

	public extern HttpStatusCode StatusCode { get; set; }

	public extern string ReasonPhrase { get; set; }

	public extern HttpResponseHeaders Headers { get; }

	public extern HttpRequestMessage RequestMessage { get; set; }

	public extern bool IsSuccessStatusCode { get; }

	public extern HttpResponseMessage();

	public extern HttpResponseMessage(HttpStatusCode statusCode);

	public extern HttpResponseMessage EnsureSuccessStatusCode();

	public override extern string ToString();

	protected virtual extern void Dispose(bool disposing);

	public extern void Dispose();
}
