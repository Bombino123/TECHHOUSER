namespace System.Net.Http.Headers;

internal class ProductInfoHeaderParser : HttpHeaderParser
{
	internal static readonly ProductInfoHeaderParser SingleValueParser;

	internal static readonly ProductInfoHeaderParser MultipleValueParser;

	public override extern bool TryParseValue(string value, object storeValue, ref int index, out object parsedValue);

	private extern ProductInfoHeaderParser();
}
