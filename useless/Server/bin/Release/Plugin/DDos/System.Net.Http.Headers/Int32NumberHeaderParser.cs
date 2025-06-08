namespace System.Net.Http.Headers;

internal class Int32NumberHeaderParser : BaseHeaderParser
{
	internal static readonly Int32NumberHeaderParser Parser;

	private extern Int32NumberHeaderParser();

	public override extern string ToString(object value);

	protected override extern int GetParsedValueLength(string value, int startIndex, object storeValue, out object parsedValue);
}
