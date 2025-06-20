using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum CommandName : byte
{
	SMB_COM_CREATE_DIRECTORY = 0,
	SMB_COM_DELETE_DIRECTORY = 1,
	SMB_COM_CLOSE = 4,
	SMB_COM_FLUSH = 5,
	SMB_COM_DELETE = 6,
	SMB_COM_RENAME = 7,
	SMB_COM_QUERY_INFORMATION = 8,
	SMB_COM_SET_INFORMATION = 9,
	SMB_COM_READ = 10,
	SMB_COM_WRITE = 11,
	SMB_COM_CHECK_DIRECTORY = 16,
	SMB_COM_WRITE_RAW = 29,
	SMB_COM_WRITE_COMPLETE = 32,
	SMB_COM_SET_INFORMATION2 = 34,
	SMB_COM_LOCKING_ANDX = 36,
	SMB_COM_TRANSACTION = 37,
	SMB_COM_TRANSACTION_SECONDARY = 38,
	SMB_COM_ECHO = 43,
	SMB_COM_OPEN_ANDX = 45,
	SMB_COM_READ_ANDX = 46,
	SMB_COM_WRITE_ANDX = 47,
	SMB_COM_TRANSACTION2 = 50,
	SMB_COM_TRANSACTION2_SECONDARY = 51,
	SMB_COM_FIND_CLOSE2 = 52,
	SMB_COM_TREE_DISCONNECT = 113,
	SMB_COM_NEGOTIATE = 114,
	SMB_COM_SESSION_SETUP_ANDX = 115,
	SMB_COM_LOGOFF_ANDX = 116,
	SMB_COM_TREE_CONNECT_ANDX = 117,
	SMB_COM_NT_TRANSACT = 160,
	SMB_COM_NT_TRANSACT_SECONDARY = 161,
	SMB_COM_NT_CREATE_ANDX = 162,
	SMB_COM_NT_CANCEL = 164,
	SMB_COM_NO_ANDX_COMMAND = byte.MaxValue
}
