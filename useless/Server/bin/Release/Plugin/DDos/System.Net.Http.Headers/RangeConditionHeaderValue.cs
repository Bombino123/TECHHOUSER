namespace System.Net.Http.Headers;

public class RangeConditionHeaderValue : ICloneable
{
	public extern DateTimeOffset? Date { get; }

	public extern EntityTagHeaderValue EntityTag { get; }

	public extern RangeConditionHeaderValue(DateTimeOffset date);

	public extern RangeConditionHeaderValue(EntityTagHeaderValue entityTag);

	public extern RangeConditionHeaderValue(string entityTag);

	private extern RangeConditionHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern RangeConditionHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out RangeConditionHeaderValue parsedValue);

	internal static extern int GetRangeConditionLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
