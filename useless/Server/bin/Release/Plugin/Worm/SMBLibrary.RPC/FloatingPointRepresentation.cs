using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public enum FloatingPointRepresentation : byte
{
	IEEE,
	VAX,
	Cray,
	IBM
}
