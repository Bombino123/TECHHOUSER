using System.Collections.Generic;

namespace System.Net.Http.Headers;

public class CacheControlHeaderValue : ICloneable
{
	public extern bool NoCache { get; set; }

	public extern ICollection<string> NoCacheHeaders { get; }

	public extern bool NoStore { get; set; }

	public extern TimeSpan? MaxAge { get; set; }

	public extern TimeSpan? SharedMaxAge { get; set; }

	public extern bool MaxStale { get; set; }

	public extern TimeSpan? MaxStaleLimit { get; set; }

	public extern TimeSpan? MinFresh { get; set; }

	public extern bool NoTransform { get; set; }

	public extern bool OnlyIfCached { get; set; }

	public extern bool Public { get; set; }

	public extern bool Private { get; set; }

	public extern ICollection<string> PrivateHeaders { get; }

	public extern bool MustRevalidate { get; set; }

	public extern bool ProxyRevalidate { get; set; }

	public extern ICollection<NameValueHeaderValue> Extensions { get; }

	public extern CacheControlHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern CacheControlHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out CacheControlHeaderValue parsedValue);

	internal static extern int GetCacheControlLength(string input, int startIndex, CacheControlHeaderValue storeValue, out CacheControlHeaderValue parsedValue);

	extern object ICloneable.Clone();
}
