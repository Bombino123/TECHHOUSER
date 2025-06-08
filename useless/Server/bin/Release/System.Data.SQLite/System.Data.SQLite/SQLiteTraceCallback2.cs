using System.Runtime.InteropServices;

namespace System.Data.SQLite;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void SQLiteTraceCallback2(SQLiteTraceFlags type, IntPtr puser, IntPtr pCtx1, IntPtr pCtx2);
