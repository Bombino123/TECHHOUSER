using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

public class HttpMessageInvoker : IDisposable
{
	public extern HttpMessageInvoker(HttpMessageHandler handler);

	public extern HttpMessageInvoker(HttpMessageHandler handler, bool disposeHandler);

	public virtual extern Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

	public extern void Dispose();

	protected virtual extern void Dispose(bool disposing);
}
