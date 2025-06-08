using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public enum ChecksumAlgorithm
{
	SHA1,
	SHA256,
	SHA384,
	SHA512
}
