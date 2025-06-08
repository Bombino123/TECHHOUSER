using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("997DD0CC-A76F-4c82-8D79-EA87559D27AD")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedSourceServerModule
{
	[PreserveSig]
	int GetSourceServerData(out int pDataByteCount, out IntPtr ppData);
}
