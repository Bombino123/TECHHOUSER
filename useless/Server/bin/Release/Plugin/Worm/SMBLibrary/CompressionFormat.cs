using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public enum CompressionFormat : ushort
{
	COMPRESSION_FORMAT_NONE,
	COMPRESSION_FORMAT_DEFAULT,
	COMPRESSION_FORMAT_LZNT1
}
