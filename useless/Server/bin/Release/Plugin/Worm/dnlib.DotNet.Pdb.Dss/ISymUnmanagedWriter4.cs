using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("BC7E3F53-F458-4C23-9DBD-A189E6E96594")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedWriter4 : ISymUnmanagedWriter3, ISymUnmanagedWriter2
{
	void _VtblGap1_29();

	void GetDebugInfoWithPadding([In][Out] ref IMAGE_DEBUG_DIRECTORY pIDD, uint cData, out uint pcData, IntPtr data);
}
