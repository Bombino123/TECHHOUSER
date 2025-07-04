using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public enum NTStatus : uint
{
	STATUS_SUCCESS = 0u,
	STATUS_PENDING = 259u,
	STATUS_NOTIFY_CLEANUP = 267u,
	STATUS_NOTIFY_ENUM_DIR = 268u,
	SEC_I_CONTINUE_NEEDED = 590610u,
	STATUS_OBJECT_NAME_EXISTS = 1073741824u,
	STATUS_BUFFER_OVERFLOW = 2147483653u,
	STATUS_NO_MORE_FILES = 2147483654u,
	SEC_E_SECPKG_NOT_FOUND = 2148074245u,
	SEC_E_INVALID_TOKEN = 2148074248u,
	STATUS_NOT_IMPLEMENTED = 3221225474u,
	STATUS_INVALID_INFO_CLASS = 3221225475u,
	STATUS_INFO_LENGTH_MISMATCH = 3221225476u,
	STATUS_INVALID_HANDLE = 3221225480u,
	STATUS_INVALID_PARAMETER = 3221225485u,
	STATUS_NO_SUCH_DEVICE = 3221225486u,
	STATUS_NO_SUCH_FILE = 3221225487u,
	STATUS_INVALID_DEVICE_REQUEST = 3221225488u,
	STATUS_END_OF_FILE = 3221225489u,
	STATUS_MORE_PROCESSING_REQUIRED = 3221225494u,
	STATUS_ACCESS_DENIED = 3221225506u,
	STATUS_BUFFER_TOO_SMALL = 3221225507u,
	STATUS_OBJECT_NAME_INVALID = 3221225523u,
	STATUS_OBJECT_NAME_NOT_FOUND = 3221225524u,
	STATUS_OBJECT_NAME_COLLISION = 3221225525u,
	STATUS_OBJECT_PATH_INVALID = 3221225529u,
	STATUS_OBJECT_PATH_NOT_FOUND = 3221225530u,
	STATUS_OBJECT_PATH_SYNTAX_BAD = 3221225531u,
	STATUS_DATA_ERROR = 3221225534u,
	STATUS_SHARING_VIOLATION = 3221225539u,
	STATUS_FILE_LOCK_CONFLICT = 3221225556u,
	STATUS_LOCK_NOT_GRANTED = 3221225557u,
	STATUS_DELETE_PENDING = 3221225558u,
	STATUS_PRIVILEGE_NOT_HELD = 3221225569u,
	STATUS_WRONG_PASSWORD = 3221225578u,
	STATUS_LOGON_FAILURE = 3221225581u,
	STATUS_ACCOUNT_RESTRICTION = 3221225582u,
	STATUS_INVALID_LOGON_HOURS = 3221225583u,
	STATUS_INVALID_WORKSTATION = 3221225584u,
	STATUS_PASSWORD_EXPIRED = 3221225585u,
	STATUS_ACCOUNT_DISABLED = 3221225586u,
	STATUS_RANGE_NOT_LOCKED = 3221225598u,
	STATUS_DISK_FULL = 3221225599u,
	STATUS_INSUFFICIENT_RESOURCES = 3221225626u,
	STATUS_MEDIA_WRITE_PROTECTED = 3221225634u,
	STATUS_FILE_IS_A_DIRECTORY = 3221225658u,
	STATUS_NOT_SUPPORTED = 3221225659u,
	STATUS_NETWORK_NAME_DELETED = 3221225673u,
	STATUS_BAD_DEVICE_TYPE = 3221225675u,
	STATUS_BAD_NETWORK_NAME = 3221225676u,
	STATUS_TOO_MANY_SESSIONS = 3221225678u,
	STATUS_REQUEST_NOT_ACCEPTED = 3221225680u,
	STATUS_DIRECTORY_NOT_EMPTY = 3221225729u,
	STATUS_NOT_A_DIRECTORY = 3221225731u,
	STATUS_TOO_MANY_OPENED_FILES = 3221225759u,
	STATUS_CANCELLED = 3221225760u,
	STATUS_CANNOT_DELETE = 3221225761u,
	STATUS_FILE_CLOSED = 3221225768u,
	STATUS_LOGON_TYPE_NOT_GRANTED = 3221225819u,
	STATUS_ACCOUNT_EXPIRED = 3221225875u,
	STATUS_FS_DRIVER_REQUIRED = 3221225884u,
	STATUS_USER_SESSION_DELETED = 3221225987u,
	STATUS_INSUFF_SERVER_RESOURCES = 3221225989u,
	STATUS_PASSWORD_MUST_CHANGE = 3221226020u,
	STATUS_NOT_FOUND = 3221226021u,
	STATUS_ACCOUNT_LOCKED_OUT = 3221226036u,
	STATUS_PATH_NOT_COVERED = 3221226071u,
	STATUS_NOT_A_REPARSE_POINT = 3221226101u,
	STATUS_INVALID_SMB = 65538u,
	STATUS_SMB_BAD_COMMAND = 1441794u,
	STATUS_SMB_BAD_FID = 393217u,
	STATUS_SMB_BAD_TID = 327682u,
	STATUS_OS2_INVALID_ACCESS = 786433u,
	STATUS_OS2_NO_MORE_SIDS = 7405569u,
	STATUS_OS2_INVALID_LEVEL = 8126465u
}
