namespace System.Net.Http.Headers;

internal class Int64NumberHeaderParser : BaseHeaderParser
{
	internal static readonly Int64NumberHeaderParser Parser;

	private extern Int64NumberHeaderParser();

	public override extern string ToString(object value);

	protected override extern int GetParsedValueLength(string value, int startIndex, object storeValue, out object parsedValue);
}
