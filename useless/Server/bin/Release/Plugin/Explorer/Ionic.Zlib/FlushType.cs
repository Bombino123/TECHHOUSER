using System.Runtime.InteropServices;

namespace Ionic.Zlib;

[ComVisible(true)]
public enum FlushType
{
	None,
	Partial,
	Sync,
	Full,
	Finish
}
