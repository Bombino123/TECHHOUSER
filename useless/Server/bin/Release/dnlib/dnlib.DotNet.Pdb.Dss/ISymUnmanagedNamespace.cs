using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("0DFF7289-54F8-11D3-BD28-0000F80849BD")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedNamespace
{
	void GetName([In] uint cchName, out uint pcchName, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);

	void GetNamespaces([In] uint cNameSpaces, out uint pcNameSpaces, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);

	void GetVariables([In] uint cVars, out uint pcVars, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] pVars);
}
