using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http;

public abstract class HttpContent : IDisposable
{
	internal const long MaxBufferSize = 2147483647L;

	internal static readonly Encoding DefaultStringEncoding;

	public extern HttpContentHeaders Headers { get; }

	protected extern HttpContent();

	public extern Task<string> ReadAsStringAsync();

	public extern Task<byte[]> ReadAsByteArrayAsync();

	public extern Task<Stream> ReadAsStreamAsync();

	protected abstract Task SerializeToStreamAsync(Stream stream, TransportContext context);

	public extern Task CopyToAsync(Stream stream, TransportContext context);

	public extern Task CopyToAsync(Stream stream);

	internal extern void CopyTo(Stream stream);

	public extern Task LoadIntoBufferAsync();

	public extern Task LoadIntoBufferAsync(long maxBufferSize);

	protected virtual extern Task<Stream> CreateContentReadStreamAsync();

	protected internal abstract bool TryComputeLength(out long length);

	protected virtual extern void Dispose(bool disposing);

	public extern void Dispose();
}
