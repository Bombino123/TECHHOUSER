namespace System.Net.Http.Headers;

public class WarningHeaderValue : ICloneable
{
	public extern int Code { get; }

	public extern string Agent { get; }

	public extern string Text { get; }

	public extern DateTimeOffset? Date { get; }

	public extern WarningHeaderValue(int code, string agent, string text);

	public extern WarningHeaderValue(int code, string agent, string text, DateTimeOffset date);

	private extern WarningHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern WarningHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out WarningHeaderValue parsedValue);

	internal static extern int GetWarningLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
