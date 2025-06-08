namespace System.Net.Http.Headers;

internal class TimeSpanHeaderParser : BaseHeaderParser
{
	internal static readonly TimeSpanHeaderParser Parser;

	private extern TimeSpanHeaderParser();

	public override extern string ToString(object value);

	protected override extern int GetParsedValueLength(string value, int startIndex, object storeValue, out object parsedValue);
}
