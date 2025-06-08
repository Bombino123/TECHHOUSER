using System.IO;
using System.Threading.Tasks;

namespace System.Net.Http;

public class StreamContent : HttpContent
{
	public extern StreamContent(Stream content);

	public extern StreamContent(Stream content, int bufferSize);

	protected override extern Task SerializeToStreamAsync(Stream stream, TransportContext context);

	protected internal override extern bool TryComputeLength(out long length);

	protected override extern void Dispose(bool disposing);

	protected override extern Task<Stream> CreateContentReadStreamAsync();
}
