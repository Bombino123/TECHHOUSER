using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public enum NegotiationResult : ushort
{
	Acceptance,
	UserRejection,
	ProviderRejection,
	NegotiateAck
}
