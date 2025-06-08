namespace System.Net.Http.Headers;

internal class TransferCodingHeaderParser : BaseHeaderParser
{
	internal static readonly TransferCodingHeaderParser SingleValueParser;

	internal static readonly TransferCodingHeaderParser MultipleValueParser;

	internal static readonly TransferCodingHeaderParser SingleValueWithQualityParser;

	internal static readonly TransferCodingHeaderParser MultipleValueWithQualityParser;

	protected override extern int GetParsedValueLength(string value, int startIndex, object storeValue, out object parsedValue);

	private extern TransferCodingHeaderParser();
}
