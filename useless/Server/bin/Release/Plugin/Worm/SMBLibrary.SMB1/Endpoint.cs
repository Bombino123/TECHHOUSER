using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum Endpoint : byte
{
	ClientSideEnd,
	ServerSideEnd
}
