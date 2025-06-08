namespace System.Net.Http.Headers;

public class EntityTagHeaderValue : ICloneable
{
	public extern string Tag { get; }

	public extern bool IsWeak { get; }

	public static extern EntityTagHeaderValue Any { get; }

	public extern EntityTagHeaderValue(string tag);

	public extern EntityTagHeaderValue(string tag, bool isWeak);

	private extern EntityTagHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern EntityTagHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out EntityTagHeaderValue parsedValue);

	internal static extern int GetEntityTagLength(string input, int startIndex, out EntityTagHeaderValue parsedValue);

	extern object ICloneable.Clone();
}
