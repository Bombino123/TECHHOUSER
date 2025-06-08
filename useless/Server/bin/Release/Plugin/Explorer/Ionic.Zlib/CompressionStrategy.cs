using System.Runtime.InteropServices;

namespace Ionic.Zlib;

[ComVisible(true)]
public enum CompressionStrategy
{
	Default,
	Filtered,
	HuffmanOnly
}
