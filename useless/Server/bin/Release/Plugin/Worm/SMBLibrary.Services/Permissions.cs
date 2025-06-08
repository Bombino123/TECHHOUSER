using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[Flags]
[ComVisible(true)]
public enum Permissions : uint
{
	PERM_FILE_READ = 1u,
	PERM_FILE_WRITE = 2u,
	PERM_FILE_CREATE = 4u,
	ACCESS_EXEC = 8u,
	ACCESS_DELETE = 0x10u,
	ACCESS_ATRIB = 0x20u,
	ACCESS_PERM = 0x40u
}
