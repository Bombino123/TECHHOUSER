using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public enum SMB2CommandName : ushort
{
	Negotiate,
	SessionSetup,
	Logoff,
	TreeConnect,
	TreeDisconnect,
	Create,
	Close,
	Flush,
	Read,
	Write,
	Lock,
	IOCtl,
	Cancel,
	Echo,
	QueryDirectory,
	ChangeNotify,
	QueryInfo,
	SetInfo,
	OplockBreak
}
