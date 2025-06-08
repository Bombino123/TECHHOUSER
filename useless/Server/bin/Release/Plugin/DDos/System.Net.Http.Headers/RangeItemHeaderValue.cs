using System.Collections.Generic;

namespace System.Net.Http.Headers;

public class RangeItemHeaderValue : ICloneable
{
	public extern long? From { get; }

	public extern long? To { get; }

	public extern RangeItemHeaderValue(long? from, long? to);

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	internal static extern int GetRangeItemListLength(string input, int startIndex, ICollection<RangeItemHeaderValue> rangeCollection);

	internal static extern int GetRangeItemLength(string input, int startIndex, out RangeItemHeaderValue parsedValue);

	extern object ICloneable.Clone();
}
