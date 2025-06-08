namespace System.Net.Http.Headers;

public sealed class TransferCodingWithQualityHeaderValue : TransferCodingHeaderValue, ICloneable
{
	public extern double? Quality { get; set; }

	internal extern TransferCodingWithQualityHeaderValue();

	public extern TransferCodingWithQualityHeaderValue(string value);

	public extern TransferCodingWithQualityHeaderValue(string value, double quality);

	extern object ICloneable.Clone();

	public new static extern TransferCodingWithQualityHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out TransferCodingWithQualityHeaderValue parsedValue);
}
