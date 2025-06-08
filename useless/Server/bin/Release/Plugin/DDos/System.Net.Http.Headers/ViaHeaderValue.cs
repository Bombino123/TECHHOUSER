namespace System.Net.Http.Headers;

public class ViaHeaderValue : ICloneable
{
	public extern string ProtocolName { get; }

	public extern string ProtocolVersion { get; }

	public extern string ReceivedBy { get; }

	public extern string Comment { get; }

	public extern ViaHeaderValue(string protocolVersion, string receivedBy);

	public extern ViaHeaderValue(string protocolVersion, string receivedBy, string protocolName);

	public extern ViaHeaderValue(string protocolVersion, string receivedBy, string protocolName, string comment);

	private extern ViaHeaderValue();

	public override extern string ToString();

	public override extern bool Equals(object obj);

	public override extern int GetHashCode();

	public static extern ViaHeaderValue Parse(string input);

	public static extern bool TryParse(string input, out ViaHeaderValue parsedValue);

	internal static extern int GetViaLength(string input, int startIndex, out object parsedValue);

	extern object ICloneable.Clone();
}
