using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("AE932FBA-3FD8-4dba-8232-30A2309B02DB")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedScope2 : ISymUnmanagedScope
{
	new void GetMethod(out ISymUnmanagedMethod pRetVal);

	new void GetParent(out ISymUnmanagedScope pRetVal);

	new void GetChildren([In] uint cChildren, out uint pcChildren, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedScope[] children);

	new void GetStartOffset(out uint pRetVal);

	new void GetEndOffset(out uint pRetVal);

	new void GetLocalCount(out uint pRetVal);

	new void GetLocals([In] uint cLocals, out uint pcLocals, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] locals);

	new void GetNamespaces([In] uint cNameSpaces, out uint pcNameSpaces, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);

	uint GetConstantCount();

	void GetConstants([In] uint cConstants, out uint pcConstants, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedConstant[] constants);
}
