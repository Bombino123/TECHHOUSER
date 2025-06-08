using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public enum EncryptionAlgorithm
{
	None,
	PkzipWeak,
	WinZipAes128,
	WinZipAes256,
	Unsupported
}
