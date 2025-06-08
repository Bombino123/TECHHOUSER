using System.Collections.Generic;

namespace System.Net.Http.Headers;

public class MediaTypeHeaderValue : ICloneable
{
	public extern string CharSet { get; set; }

	public extern ICollection<NameValueHeaderValue> Parameters { get; }

	public extern string MediaType { get; set; }

	internal extern MediaTypeHeaderValue();

	protected extern MediaTypeHeaderValue(MediaTypeHeaderValue source);

	public extern MediaTypeHeaderValue(string mediaType);

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern MediaTypeHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out MediaTypeHeaderValue parsedValue);

	internal static extern int GetMediaTypeLength(string input, int startIndex, Func<MediaTypeHeaderValue> mediaTypeCreator, out MediaTypeHeaderValue parsedValue);

	extern object ICloneable.Clone();
}
