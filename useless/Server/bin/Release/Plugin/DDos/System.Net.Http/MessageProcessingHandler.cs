using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

public abstract class MessageProcessingHandler : DelegatingHandler
{
	protected extern MessageProcessingHandler();

	protected extern MessageProcessingHandler(HttpMessageHandler innerHandler);

	protected abstract HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken);

	protected abstract HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken);

	protected internal sealed override extern Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
