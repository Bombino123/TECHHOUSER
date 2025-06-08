namespace System.Net.Http.Headers;

internal class ByteArrayHeaderParser : HttpHeaderParser
{
	internal static readonly ByteArrayHeaderParser Parser;

	private extern ByteArrayHeaderParser();

	public override extern string ToString(object value);

	public override extern bool TryParseValue(string value, object storeValue, ref int index, out object parsedValue);
}
