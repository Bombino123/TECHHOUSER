using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("5BA52F3B-6BF8-40FC-B476-D39C529B331E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedWriter8 : ISymUnmanagedWriter7, ISymUnmanagedWriter6, ISymUnmanagedWriter5, ISymUnmanagedWriter4, ISymUnmanagedWriter3, ISymUnmanagedWriter2
{
	void _VtblGap1_35();

	void UpdateSignature(Guid pdbId, uint stamp, uint age);

	void SetSourceServerData(IntPtr data, uint cData);

	void SetSourceLinkData(IntPtr data, uint cData);
}
