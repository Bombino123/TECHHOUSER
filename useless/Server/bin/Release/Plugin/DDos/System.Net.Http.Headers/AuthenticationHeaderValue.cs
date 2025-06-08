namespace System.Net.Http.Headers;

public class AuthenticationHeaderValue : ICloneable
{
	public extern string Scheme { get; }

	public extern string Parameter { get; }

	public extern AuthenticationHeaderValue(string scheme);

	public extern AuthenticationHeaderValue(string scheme, string parameter);

	private extern AuthenticationHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern AuthenticationHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out AuthenticationHeaderValue parsedValue);

	internal static extern int GetAuthenticationLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
