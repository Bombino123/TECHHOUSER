using System.Collections.Generic;
using System.Text;

namespace System.Net.Http.Headers;

public class NameValueHeaderValue : ICloneable
{
	public extern string Name { get; }

	public extern string Value { get; set; }

	internal extern NameValueHeaderValue();

	public extern NameValueHeaderValue(string name);

	public extern NameValueHeaderValue(string name, string value);

	protected extern NameValueHeaderValue(NameValueHeaderValue source);

	public override extern int GetHashCode();

	public override extern bool Equals(object obj);

	public static extern NameValueHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out NameValueHeaderValue parsedValue);

	public override extern string ToString();

	internal static extern void ToString(ICollection<NameValueHeaderValue> values, char separator, bool leadingSeparator, StringBuilder destination);

	internal static extern string ToString(ICollection<NameValueHeaderValue> values, char separator, bool leadingSeparator);

	internal static extern int GetHashCode(ICollection<NameValueHeaderValue> values);

	internal static extern int GetNameValueLength(string input, int startIndex, out NameValueHeaderValue parsedValue);

	internal static extern int GetNameValueLength(string input, int startIndex, Func<NameValueHeaderValue> nameValueCreator, out NameValueHeaderValue parsedValue);

	internal static extern int GetNameValueListLength(string input, int startIndex, char delimiter, ICollection<NameValueHeaderValue> nameValueCollection);

	internal static extern NameValueHeaderValue Find(ICollection<NameValueHeaderValue> values, string name);

	internal static extern int GetValueLength(string input, int startIndex);

	extern object ICloneable.Clone();
}
