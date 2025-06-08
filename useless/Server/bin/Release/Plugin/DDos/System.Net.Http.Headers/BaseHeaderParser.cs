namespace System.Net.Http.Headers;

internal abstract class BaseHeaderParser : HttpHeaderParser
{
	protected extern BaseHeaderParser(bool supportsMultipleValues);

	protected abstract int GetParsedValueLength(string value, int startIndex, object storeValue, out object parsedValue);

	public sealed override extern bool TryParseValue(string value, object storeValue, ref int index, out object parsedValue);
}
