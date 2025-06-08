using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace System.Net.Http;

public class MultipartContent : HttpContent, IEnumerable<HttpContent>, IEnumerable
{
	public extern MultipartContent();

	public extern MultipartContent(string subtype);

	public extern MultipartContent(string subtype, string boundary);

	public virtual extern void Add(HttpContent content);

	protected override extern void Dispose(bool disposing);

	public extern IEnumerator<HttpContent> GetEnumerator();

	extern IEnumerator IEnumerable.GetEnumerator();

	protected override extern Task SerializeToStreamAsync(Stream stream, TransportContext context);

	protected internal override extern bool TryComputeLength(out long length);
}
