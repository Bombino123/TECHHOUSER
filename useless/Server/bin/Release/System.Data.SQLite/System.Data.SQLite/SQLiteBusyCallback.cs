using System.Runtime.InteropServices;

namespace System.Data.SQLite;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate SQLiteBusyReturnCode SQLiteBusyCallback(IntPtr pUserData, int count);
