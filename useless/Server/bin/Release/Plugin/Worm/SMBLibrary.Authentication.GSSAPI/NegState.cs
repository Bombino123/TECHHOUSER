using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public enum NegState : byte
{
	AcceptCompleted,
	AcceptIncomplete,
	Reject,
	RequestMic
}
