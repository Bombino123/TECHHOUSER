using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum NTTransactSubcommandName : ushort
{
	NT_TRANSACT_CREATE = 1,
	NT_TRANSACT_IOCTL = 2,
	NT_TRANSACT_SET_SECURITY_DESC = 3,
	NT_TRANSACT_NOTIFY_CHANGE = 4,
	NT_TRANSACT_QUERY_SECURITY_DESC = 6,
	NT_TRANSACT_QUERY_QUOTA = 7,
	NT_TRANSACT_SET_QUOTA = 8
}
