namespace System.Net.Http.Headers;

internal class UriHeaderParser : HttpHeaderParser
{
	internal static readonly UriHeaderParser RelativeOrAbsoluteUriParser;

	public override extern bool TryParseValue(string value, object storeValue, ref int index, out object parsedValue);

	public override extern string ToString(object value);

	private extern UriHeaderParser();
}
