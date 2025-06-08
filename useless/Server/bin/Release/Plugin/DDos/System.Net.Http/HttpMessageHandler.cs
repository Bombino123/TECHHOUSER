using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

public abstract class HttpMessageHandler : IDisposable
{
	protected extern HttpMessageHandler();

	protected internal abstract Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

	protected virtual extern void Dispose(bool disposing);

	public extern void Dispose();
}
