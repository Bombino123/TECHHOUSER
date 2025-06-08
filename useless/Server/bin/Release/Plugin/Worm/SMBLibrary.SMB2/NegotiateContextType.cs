using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public enum NegotiateContextType : ushort
{
	SMB2_PREAUTH_INTEGRITY_CAPABILITIES = 1,
	SMB2_ENCRYPTION_CAPABILITIES
}
