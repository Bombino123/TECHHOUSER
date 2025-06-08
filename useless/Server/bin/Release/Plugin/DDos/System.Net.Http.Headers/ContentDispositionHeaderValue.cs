using System.Collections.Generic;

namespace System.Net.Http.Headers;

public class ContentDispositionHeaderValue : ICloneable
{
	public extern string DispositionType { get; set; }

	public extern ICollection<NameValueHeaderValue> Parameters { get; }

	public extern string Name { get; set; }

	public extern string FileName { get; set; }

	public extern string FileNameStar { get; set; }

	public extern DateTimeOffset? CreationDate { get; set; }

	public extern DateTimeOffset? ModificationDate { get; set; }

	public extern DateTimeOffset? ReadDate { get; set; }

	public extern long? Size { get; set; }

	internal extern ContentDispositionHeaderValue();

	protected extern ContentDispositionHeaderValue(ContentDispositionHeaderValue source);

	public extern ContentDispositionHeaderValue(string dispositionType);

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	extern object ICloneable.Clone();

	public static extern ContentDispositionHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out ContentDispositionHeaderValue parsedValue);

	internal static extern int GetDispositionTypeLength(string input, int startIndex, out object parsedValue);
}
