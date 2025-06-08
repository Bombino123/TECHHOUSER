namespace System.Net.Http;

public class HttpMethod : IEquatable<HttpMethod>
{
	public static extern HttpMethod Get { get; }

	public static extern HttpMethod Put { get; }

	public static extern HttpMethod Post { get; }

	public static extern HttpMethod Delete { get; }

	public static extern HttpMethod Head { get; }

	public static extern HttpMethod Options { get; }

	public static extern HttpMethod Trace { get; }

	public extern string Method { get; }

	public extern HttpMethod(string method);

	public extern bool Equals(HttpMethod other);

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public override extern string ToString();

	public static extern bool operator ==(HttpMethod left, HttpMethod right);

	public static extern bool operator !=(HttpMethod left, HttpMethod right);
}
