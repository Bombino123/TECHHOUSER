using System.IO;
using System.Threading.Tasks;

namespace System.Net.Http;

public class ByteArrayContent : HttpContent
{
	public extern ByteArrayContent(byte[] content);

	public extern ByteArrayContent(byte[] content, int offset, int count);

	protected override extern Task SerializeToStreamAsync(Stream stream, TransportContext context);

	protected internal override extern bool TryComputeLength(out long length);

	protected override extern Task<Stream> CreateContentReadStreamAsync();
}
