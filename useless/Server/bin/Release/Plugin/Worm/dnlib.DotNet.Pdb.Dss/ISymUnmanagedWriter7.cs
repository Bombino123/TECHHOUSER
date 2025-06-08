using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("22DAEAF2-70F6-4EF1-B0C3-984F0BF27BFD")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedWriter7 : ISymUnmanagedWriter6, ISymUnmanagedWriter5, ISymUnmanagedWriter4, ISymUnmanagedWriter3, ISymUnmanagedWriter2
{
	void _VtblGap1_34();

	void UpdateSignatureByHashingContent(IntPtr buffer, uint cData);
}
