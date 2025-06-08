using System.Collections.Generic;

namespace System.Net.Http.Headers;

public sealed class HttpRequestHeaders : HttpHeaders
{
	public extern HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accept { get; }

	public extern HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptCharset { get; }

	public extern HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptEncoding { get; }

	public extern HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptLanguage { get; }

	public extern AuthenticationHeaderValue Authorization { get; set; }

	public extern HttpHeaderValueCollection<NameValueWithParametersHeaderValue> Expect { get; }

	public extern bool? ExpectContinue { get; set; }

	public extern string From { get; set; }

	public extern string Host { get; set; }

	public extern HttpHeaderValueCollection<EntityTagHeaderValue> IfMatch { get; }

	public extern DateTimeOffset? IfModifiedSince { get; set; }

	public extern HttpHeaderValueCollection<EntityTagHeaderValue> IfNoneMatch { get; }

	public extern RangeConditionHeaderValue IfRange { get; set; }

	public extern DateTimeOffset? IfUnmodifiedSince { get; set; }

	public extern int? MaxForwards { get; set; }

	public extern AuthenticationHeaderValue ProxyAuthorization { get; set; }

	public extern RangeHeaderValue Range { get; set; }

	public extern Uri Referrer { get; set; }

	public extern HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue> TE { get; }

	public extern HttpHeaderValueCollection<ProductInfoHeaderValue> UserAgent { get; }

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

	internal extern HttpRequestHeaders();

	static extern HttpRequestHeaders();

	internal static extern void AddKnownHeaders(HashSet<string> headerSet);

	internal override extern void AddHeaders(HttpHeaders sourceHeaders);
}
