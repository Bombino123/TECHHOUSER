using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum NamedPipeState : ushort
{
	DisconnectedByServer = 1,
	Listening,
	ConnectionToServerOK,
	ServerEndClosed
}
