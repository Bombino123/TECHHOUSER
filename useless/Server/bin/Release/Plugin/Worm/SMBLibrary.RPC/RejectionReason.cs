using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public enum RejectionReason : ushort
{
	NotSpecified,
	AbstractSyntaxNotSupported,
	ProposedTransferSyntaxesNotSupported,
	LocalLimitExceeded
}
