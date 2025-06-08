using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public enum CompressionMethod
{
	None = 0,
	Deflate = 8,
	BZip2 = 12
}
