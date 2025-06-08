using System.Runtime.InteropServices;

namespace SMBLibrary.Client;

[ComVisible(true)]
public enum AuthenticationMethod
{
	NTLMv1,
	NTLMv1ExtendedSessionSecurity,
	NTLMv2
}
