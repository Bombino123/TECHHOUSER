using System.Collections.Generic;

namespace System.Net.Http.Headers;

public class TransferCodingHeaderValue : ICloneable
{
	public extern string Value { get; }

	public extern ICollection<NameValueHeaderValue> Parameters { get; }

	internal extern TransferCodingHeaderValue();

	protected extern TransferCodingHeaderValue(TransferCodingHeaderValue source);

	public extern TransferCodingHeaderValue(string value);

	public static extern TransferCodingHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out TransferCodingHeaderValue parsedValue);

	internal static extern int GetTransferCodingLength(string input, int startIndex, Func<TransferCodingHeaderValue> transferCodingCreator, out TransferCodingHeaderValue parsedValue);

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	extern object ICloneable.Clone();
}
