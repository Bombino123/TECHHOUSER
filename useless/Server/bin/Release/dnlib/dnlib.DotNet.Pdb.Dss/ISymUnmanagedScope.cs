using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("68005D0F-B8E0-3B01-84D5-A11A94154942")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedScope
{
	void GetMethod(out ISymUnmanagedMethod pRetVal);

	void GetParent(out ISymUnmanagedScope pRetVal);

	void GetChildren([In] uint cChildren, out uint pcChildren, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedScope[] children);

	void GetStartOffset(out uint pRetVal);

	void GetEndOffset(out uint pRetVal);

	void GetLocalCount(out uint pRetVal);

	void GetLocals([In] uint cLocals, out uint pcLocals, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] locals);

	void GetNamespaces([In] uint cNameSpaces, out uint pcNameSpaces, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);
}
