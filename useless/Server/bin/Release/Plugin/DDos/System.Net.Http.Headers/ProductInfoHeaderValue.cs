namespace System.Net.Http.Headers;

public class ProductInfoHeaderValue : ICloneable
{
	public extern ProductHeaderValue Product { get; }

	public extern string Comment { get; }

	public extern ProductInfoHeaderValue(string productName, string productVersion);

	public extern ProductInfoHeaderValue(ProductHeaderValue product);

	public extern ProductInfoHeaderValue(string comment);

	private extern ProductInfoHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern ProductInfoHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out ProductInfoHeaderValue parsedValue);

	internal static extern int GetProductInfoLength(string input, int startIndex, out ProductInfoHeaderValue parsedValue);

	extern object ICloneable.Clone();
}
