namespace System.Net.Http.Headers;

internal class DateHeaderParser : HttpHeaderParser
{
	internal static readonly DateHeaderParser Parser;

	private extern DateHeaderParser();

	public override extern string ToString(object value);

	public override extern bool TryParseValue(string value, object storeValue, ref int index, out object parsedValue);
}
