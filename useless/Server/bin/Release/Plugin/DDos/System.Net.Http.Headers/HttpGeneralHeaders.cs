using System.Collections.Generic;

namespace System.Net.Http.Headers;

internal sealed class HttpGeneralHeaders
{
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

	internal extern HttpGeneralHeaders(HttpHeaders parent);

	internal static extern void AddParsers(Dictionary<string, HttpHeaderParser> parserStore);

	internal static extern void AddKnownHeaders(HashSet<string> headerSet);

	internal extern void AddSpecialsFrom(HttpGeneralHeaders sourceHeaders);
}
