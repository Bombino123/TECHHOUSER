using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public enum CharacterFormat : byte
{
	ASCII,
	EBCDIC
}
