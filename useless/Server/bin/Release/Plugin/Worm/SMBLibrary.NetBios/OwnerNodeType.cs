using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public enum OwnerNodeType : byte
{
	BNode = 0,
	PNode = 1,
	MNode = 0x10
}
