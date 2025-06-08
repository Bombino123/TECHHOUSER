namespace System.Net.Http.Headers;

public class ContentRangeHeaderValue : ICloneable
{
	public extern string Unit { get; set; }

	public extern long? From { get; }

	public extern long? To { get; }

	public extern long? Length { get; }

	public extern bool HasLength { get; }

	public extern bool HasRange { get; }

	public extern ContentRangeHeaderValue(long from, long to, long length);

	public extern ContentRangeHeaderValue(long length);

	public extern ContentRangeHeaderValue(long from, long to);

	private extern ContentRangeHeaderValue();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public override extern string ToString();

	public static extern ContentRangeHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out ContentRangeHeaderValue parsedValue);

	internal static extern int GetContentRangeLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
