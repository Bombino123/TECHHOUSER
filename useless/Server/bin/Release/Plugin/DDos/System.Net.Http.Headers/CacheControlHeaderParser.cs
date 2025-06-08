namespace System.Net.Http.Headers;

internal class CacheControlHeaderParser : BaseHeaderParser
{
	internal static readonly CacheControlHeaderParser Parser;

	private extern CacheControlHeaderParser();

	protected override extern int GetParsedValueLength(string value, int startIndex, object storeValue, out object parsedValue);
}
