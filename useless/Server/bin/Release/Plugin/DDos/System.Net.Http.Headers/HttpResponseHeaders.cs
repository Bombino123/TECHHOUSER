using System.Collections.Generic;

namespace System.Net.Http.Headers;

public sealed class HttpResponseHeaders : HttpHeaders
{
	public extern HttpHeaderValueCollection<string> AcceptRanges { get; }

	public extern TimeSpan? Age { get; set; }

	public extern EntityTagHeaderValue ETag { get; set; }

	public extern Uri Location { get; set; }

	public extern HttpHeaderValueCollection<AuthenticationHeaderValue> ProxyAuthenticate { get; }

	public extern RetryConditionHeaderValue RetryAfter { get; set; }

	public extern HttpHeaderValueCollection<ProductInfoHeaderValue> Server { get; }

	public extern HttpHeaderValueCollection<string> Vary { get; }

	public extern HttpHeaderValueCollection<AuthenticationHeaderValue> WwwAuthenticate { get; }

	public extern CacheControlHeaderValue CacheControl { get; set; }

	public extern HttpHeaderValueCollection<string> Connection { get; }

	public extern bool? ConnectionClose { get; set; }

	public extern DateTimeOffset? Date { get; set; }

	public extern HttpHeaderValueCollection<NameValueHeaderValue> Pragma { get; }

	public extern HttpHeaderValueCollection<string> Trailer { get; }

	public extern HttpHeaderValueCollection<TransferCodingHeaderValue> TransferEncoding { get; }

	public extern bool? TransferEncodingChunked { get; set; }

	public extern HttpHeaderValueCollection<ProductHeaderValue> Upgrade { get; }

	public extern HttpHeaderValueCollection<ViaHeaderValue> Via { get; }

	public extern HttpHeaderValueCollection<WarningHeaderValue> Warning { get; }

	internal extern HttpResponseHeaders();

	static extern HttpResponseHeaders();

	internal static extern void AddKnownHeaders(HashSet<string> headerSet);

	internal override extern void AddHeaders(HttpHeaders sourceHeaders);
}
