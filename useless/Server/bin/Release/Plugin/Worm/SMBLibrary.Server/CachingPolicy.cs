using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public enum CachingPolicy
{
	ManualCaching,
	AutoCaching,
	VideoCaching,
	NoCaching
}
