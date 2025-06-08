using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum SecurityFlags : byte
{
	SMB_SECURITY_CONTEXT_TRACKING = 1,
	SMB_SECURITY_EFFECTIVE_ONLY
}
