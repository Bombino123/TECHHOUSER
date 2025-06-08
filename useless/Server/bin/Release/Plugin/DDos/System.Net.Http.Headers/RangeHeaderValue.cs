using System.Collections.Generic;

namespace System.Net.Http.Headers;

public class RangeHeaderValue : ICloneable
{
	public extern string Unit { get; set; }

	public extern ICollection<RangeItemHeaderValue> Ranges { get; }

	public extern RangeHeaderValue();

	public extern RangeHeaderValue(long? from, long? to);

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern RangeHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out RangeHeaderValue parsedValue);

	internal static extern int GetRangeLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
