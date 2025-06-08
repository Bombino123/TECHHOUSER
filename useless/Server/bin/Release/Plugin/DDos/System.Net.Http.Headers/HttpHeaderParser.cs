using System.Collections;

namespace System.Net.Http.Headers;

internal abstract class HttpHeaderParser
{
	internal const string DefaultSeparator = ", ";

	public extern bool SupportsMultipleValues { get; }

	public extern string Separator { get; }

	public virtual extern IEqualityComparer Comparer { get; }

	protected extern HttpHeaderParser(bool supportsMultipleValues);

	protected extern HttpHeaderParser(bool supportsMultipleValues, string separator);

	public abstract bool TryParseValue(string value, object storeValue, ref int index, out object parsedValue);

	public extern object ParseValue(string value, object storeValue, ref int index);

	public virtual extern string ToString(object value);
}
