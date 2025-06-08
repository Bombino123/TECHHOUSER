namespace System.Net.Http.Headers;

public class StringWithQualityHeaderValue : ICloneable
{
	public extern string Value { get; }

	public extern double? Quality { get; }

	public extern StringWithQualityHeaderValue(string value);

	public extern StringWithQualityHeaderValue(string value, double quality);

	private extern StringWithQualityHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern StringWithQualityHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out StringWithQualityHeaderValue parsedValue);

	internal static extern int GetStringWithQualityLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
