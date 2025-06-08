namespace System.Net.Http.Headers;

internal class MediaTypeHeaderParser : BaseHeaderParser
{
	internal static readonly MediaTypeHeaderParser SingleValueParser;

	internal static readonly MediaTypeHeaderParser SingleValueWithQualityParser;

	internal static readonly MediaTypeHeaderParser MultipleValuesParser;

	protected override extern int GetParsedValueLength(string value, int startIndex, object storeValue, out object parsedValue);

	private extern MediaTypeHeaderParser();
}
