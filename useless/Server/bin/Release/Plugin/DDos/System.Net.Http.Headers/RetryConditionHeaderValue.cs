namespace System.Net.Http.Headers;

public class RetryConditionHeaderValue : ICloneable
{
	public extern DateTimeOffset? Date { get; }

	public extern TimeSpan? Delta { get; }

	public extern RetryConditionHeaderValue(DateTimeOffset date);

	public extern RetryConditionHeaderValue(TimeSpan delta);

	private extern RetryConditionHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern RetryConditionHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out RetryConditionHeaderValue parsedValue);

	internal static extern int GetRetryConditionLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
