using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("CA6C2ED9-103D-46A9-B03B-05446485848B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedWriter6 : ISymUnmanagedWriter5, ISymUnmanagedWriter4, ISymUnmanagedWriter3, ISymUnmanagedWriter2
{
	void _VtblGap1_33();

	void InitializeDeterministic([MarshalAs(UnmanagedType.IUnknown)] object emitter, [MarshalAs(UnmanagedType.IUnknown)] object stream);
}
