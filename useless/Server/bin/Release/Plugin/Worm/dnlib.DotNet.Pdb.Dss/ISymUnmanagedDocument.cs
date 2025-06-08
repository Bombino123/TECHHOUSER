using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedDocument
{
	void GetURL([In] uint cchUrl, out uint pcchUrl, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szUrl);

	void GetDocumentType(out Guid pRetVal);

	void GetLanguage(out Guid pRetVal);

	void GetLanguageVendor(out Guid pRetVal);

	void GetCheckSumAlgorithmId(out Guid pRetVal);

	void GetCheckSum([In] uint cData, out uint pcData, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] data);

	void FindClosestLine([In] uint line, out uint pRetVal);

	void HasEmbeddedSource(out bool pRetVal);

	[PreserveSig]
	int GetSourceLength(out int pRetVal);

	[PreserveSig]
	int GetSourceRange([In] uint startLine, [In] uint startColumn, [In] uint endLine, [In] uint endColumn, [In] int cSourceBytes, out int pcSourceBytes, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] source);
}
