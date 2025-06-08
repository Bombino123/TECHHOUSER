using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

public abstract class DelegatingHandler : HttpMessageHandler
{
	public extern HttpMessageHandler InnerHandler { get; set; }

	protected extern DelegatingHandler();

	protected extern DelegatingHandler(HttpMessageHandler innerHandler);

	protected internal override extern Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

	protected override extern void Dispose(bool disposing);
}
