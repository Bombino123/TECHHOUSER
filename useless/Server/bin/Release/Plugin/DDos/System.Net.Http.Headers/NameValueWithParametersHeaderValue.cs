using System.Collections.Generic;

namespace System.Net.Http.Headers;

public class NameValueWithParametersHeaderValue : NameValueHeaderValue, ICloneable
{
	public extern ICollection<NameValueHeaderValue> Parameters { get; }

	public extern NameValueWithParametersHeaderValue(string name);

	public extern NameValueWithParametersHeaderValue(string name, string value);

	internal extern NameValueWithParametersHeaderValue();

	protected extern NameValueWithParametersHeaderValue(NameValueWithParametersHeaderValue source);

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public override extern string ToString();

	public new static extern NameValueWithParametersHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out NameValueWithParametersHeaderValue parsedValue);

	internal static extern int GetNameValueWithParametersLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
