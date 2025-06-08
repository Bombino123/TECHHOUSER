namespace System.Net.Http.Headers;

public class ProductHeaderValue : ICloneable
{
	public extern string Name { get; }

	public extern string Version { get; }

	public extern ProductHeaderValue(string name);

	public extern ProductHeaderValue(string name, string version);

	private extern ProductHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern ProductHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out ProductHeaderValue parsedValue);

	internal static extern int GetProductLength(string input, int startIndex, out ProductHeaderValue parsedValue);

	extern object ICloneable.Clone();
}
