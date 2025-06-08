using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("B62B923C-B500-3158-A543-24F307A8B7E1")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedMethod
{
	void GetToken(out uint pToken);

	void GetSequencePointCount(out uint pRetVal);

	void GetRootScope(out ISymUnmanagedScope pRetVal);

	void GetScopeFromOffset([In] uint offset, out ISymUnmanagedScope pRetVal);

	void GetOffset([In] ISymUnmanagedDocument document, [In] uint line, [In] uint column, out uint pRetVal);

	void GetRanges([In] ISymUnmanagedDocument document, [In] uint line, [In] uint column, [In] uint cRanges, out uint pcRanges, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] ranges);

	void GetParameters([In] uint cParams, out uint pcParams, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] parameters);

	void GetNamespace(out ISymUnmanagedNamespace pRetVal);

	void GetSourceStartEnd([In] ISymUnmanagedDocument[] docs, [In] int[] lines, [In] int[] columns, out bool pRetVal);

	void GetSequencePoints([In] uint cPoints, out uint pcPoints, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] offsets, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedDocument[] documents, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] lines, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] columns, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] endLines, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] endColumns);
}
