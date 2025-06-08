using System.Collections;

namespace System.Net.Http.Headers;

internal sealed class GenericHeaderParser : BaseHeaderParser
{
	internal static readonly HttpHeaderParser HostParser;

	internal static readonly HttpHeaderParser TokenListParser;

	internal static readonly HttpHeaderParser SingleValueNameValueWithParametersParser;

	internal static readonly HttpHeaderParser MultipleValueNameValueWithParametersParser;

	internal static readonly HttpHeaderParser SingleValueNameValueParser;

	internal static readonly HttpHeaderParser MultipleValueNameValueParser;

	internal static readonly HttpHeaderParser MailAddressParser;

	internal static readonly HttpHeaderParser SingleValueProductParser;

	internal static readonly HttpHeaderParser MultipleValueProductParser;

	internal static readonly HttpHeaderParser RangeConditionParser;

	internal static readonly HttpHeaderParser SingleValueAuthenticationParser;

	internal static readonly HttpHeaderParser MultipleValueAuthenticationParser;

	internal static readonly HttpHeaderParser RangeParser;

	internal static readonly HttpHeaderParser RetryConditionParser;

	internal static readonly HttpHeaderParser ContentRangeParser;

	internal static readonly HttpHeaderParser ContentDispositionParser;

	internal static readonly HttpHeaderParser SingleValueStringWithQualityParser;

	internal static readonly HttpHeaderParser MultipleValueStringWithQualityParser;

	internal static readonly HttpHeaderParser SingleValueEntityTagParser;

	internal static readonly HttpHeaderParser MultipleValueEntityTagParser;

	internal static readonly HttpHeaderParser SingleValueViaParser;

	internal static readonly HttpHeaderParser MultipleValueViaParser;

	internal static readonly HttpHeaderParser SingleValueWarningParser;

	internal static readonly HttpHeaderParser MultipleValueWarningParser;

	public override extern IEqualityComparer Comparer { get; }

	protected override extern int GetParsedValueLength(string value, int startIndex, object storeValue, out object parsedValue);

	private extern GenericHeaderParser();
}
