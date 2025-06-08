using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public enum FaultStatus : uint
{
	OpRangeError = 469827586u,
	UnknownInterface = 469827587u,
	RPCVersionMismatch = 469762056u,
	ProtocolError = 469827595u
}
