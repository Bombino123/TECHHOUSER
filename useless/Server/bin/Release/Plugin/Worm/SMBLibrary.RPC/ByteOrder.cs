using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public enum ByteOrder : byte
{
	BigEndian,
	LittleEndian
}
