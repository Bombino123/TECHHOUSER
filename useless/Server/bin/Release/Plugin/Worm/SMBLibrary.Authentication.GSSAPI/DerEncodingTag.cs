using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public enum DerEncodingTag : byte
{
	ByteArray = 4,
	ObjectIdentifier = 6,
	Enum = 10,
	GeneralString = 27,
	Sequence = 48
}
