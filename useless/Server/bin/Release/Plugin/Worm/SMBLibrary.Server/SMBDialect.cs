using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public enum SMBDialect
{
	NotSet,
	NTLM012,
	SMB202,
	SMB210,
	SMB300
}
