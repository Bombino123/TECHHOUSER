using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum ReferenceLocality : byte
{
	Unknown,
	Sequential,
	Random,
	RandomWithLocality
}
