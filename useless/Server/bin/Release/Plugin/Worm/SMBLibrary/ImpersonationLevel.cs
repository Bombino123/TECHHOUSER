using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public enum ImpersonationLevel : uint
{
	Anonymous,
	Identification,
	Impersonation,
	Delegation
}
