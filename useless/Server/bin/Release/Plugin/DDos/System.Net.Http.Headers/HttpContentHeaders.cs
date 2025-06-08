using System.Collections.Generic;

namespace System.Net.Http.Headers;

public sealed class HttpContentHeaders : HttpHeaders
{
	public extern ICollection<string> Allow { get; }

	public extern ContentDispositionHeaderValue ContentDisposition { get; set; }

	public extern ICollection<string> ContentEncoding { get; }

	public extern ICollection<string> ContentLanguage { get; }

	public extern long? ContentLength { get; set; }

	public extern Uri ContentLocation { get; set; }

	public extern byte[] ContentMD5 { get; set; }

	public extern ContentRangeHeaderValue ContentRange { get; set; }

	public extern MediaTypeHeaderValue ContentType { get; set; }

	public extern DateTimeOffset? Expires { get; set; }

	public extern DateTimeOffset? LastModified { get; set; }

	internal extern HttpContentHeaders(Func<long?> calculateLengthFunc);

	static extern HttpContentHeaders();

	internal static extern void AddKnownHeaders(HashSet<string> headerSet);
}
