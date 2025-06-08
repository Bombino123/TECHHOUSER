using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public enum MessageTypeName : uint
{
	Negotiate = 1u,
	Challenge,
	Authenticate
}
