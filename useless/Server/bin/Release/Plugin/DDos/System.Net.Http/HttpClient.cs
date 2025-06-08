using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

public class HttpClient : HttpMessageInvoker
{
	public extern HttpRequestHeaders DefaultRequestHeaders { get; }

	public extern Uri BaseAddress { get; set; }

	public extern TimeSpan Timeout { get; set; }

	public extern long MaxResponseContentBufferSize { get; set; }

	public extern HttpClient();

	public extern HttpClient(HttpMessageHandler handler);

	public extern HttpClient(HttpMessageHandler handler, bool disposeHandler);

	public extern Task<string> GetStringAsync(string requestUri);

	public extern Task<string> GetStringAsync(Uri requestUri);

	public extern Task<byte[]> GetByteArrayAsync(string requestUri);

	public extern Task<byte[]> GetByteArrayAsync(Uri requestUri);

	public extern Task<Stream> GetStreamAsync(string requestUri);

	public extern Task<Stream> GetStreamAsync(Uri requestUri);

	public extern Task<HttpResponseMessage> GetAsync(string requestUri);

	public extern Task<HttpResponseMessage> GetAsync(Uri requestUri);

	public extern Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption);

	public extern Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption);

	public extern Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);

	public extern Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content);

	public extern Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content);

	public extern Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content);

	public extern Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> DeleteAsync(string requestUri);

	public extern Task<HttpResponseMessage> DeleteAsync(Uri requestUri);

	public extern Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> DeleteAsync(Uri requestUri, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

	public override extern Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

	public extern Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption);

	public extern Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken);

	public extern void CancelPendingRequests();

	protected override extern void Dispose(bool disposing);
}
