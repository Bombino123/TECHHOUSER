using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[ComVisible(true)]
public enum PlatformName : uint
{
	DOS = 300u,
	OS2 = 400u,
	NT = 500u,
	OSF = 600u,
	VMS = 700u
}
