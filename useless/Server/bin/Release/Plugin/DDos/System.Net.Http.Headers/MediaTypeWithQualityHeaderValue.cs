namespace System.Net.Http.Headers;

public sealed class MediaTypeWithQualityHeaderValue : MediaTypeHeaderValue, ICloneable
{
	public extern double? Quality { get; set; }

	internal extern MediaTypeWithQualityHeaderValue();

	public extern MediaTypeWithQualityHeaderValue(string mediaType);

	public extern MediaTypeWithQualityHeaderValue(string mediaType, double quality);

	extern object ICloneable.Clone();

	public new static extern MediaTypeWithQualityHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out MediaTypeWithQualityHeaderValue parsedValue);
}
