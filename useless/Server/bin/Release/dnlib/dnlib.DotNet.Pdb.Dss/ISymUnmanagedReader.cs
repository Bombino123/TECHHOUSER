using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedReader
{
	void GetDocument([In][MarshalAs(UnmanagedType.LPWStr)] string url, [In] Guid language, [In] Guid languageVendor, [In] Guid documentType, out ISymUnmanagedDocument pRetVal);

	void GetDocuments([In] uint cDocs, out uint pcDocs, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedDocument[] pDocs);

	[PreserveSig]
	int GetUserEntryPoint(out uint pToken);

	void GetMethod([In] uint token, out ISymUnmanagedMethod retVal);

	[PreserveSig]
	int GetMethodByVersion([In] uint token, [In] int version, out ISymUnmanagedMethod pRetVal);

	void GetVariables([In] uint parent, [In] uint cVars, out uint pcVars, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ISymUnmanagedVariable[] pVars);

	void GetGlobalVariables([In] uint cVars, out uint pcVars, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] pVars);

	void GetMethodFromDocumentPosition([In] ISymUnmanagedDocument document, [In] uint line, [In] uint column, out ISymUnmanagedMethod pRetVal);

	void GetSymAttribute([In] uint parent, [In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] uint cBuffer, out uint pcBuffer, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer);

	void GetNamespaces([In] uint cNameSpaces, out uint pcNameSpaces, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);

	[PreserveSig]
	int Initialize([In][MarshalAs(UnmanagedType.IUnknown)] object importer, [In][MarshalAs(UnmanagedType.LPWStr)] string filename, [In][MarshalAs(UnmanagedType.LPWStr)] string searchPath, [In] IStream pIStream);

	void UpdateSymbolStore([In][MarshalAs(UnmanagedType.LPWStr)] string filename, [In] IStream pIStream);

	void ReplaceSymbolStore([In][MarshalAs(UnmanagedType.LPWStr)] string filename, [In] IStream pIStream);

	void GetSymbolStoreFileName([In] uint cchName, out uint pcchName, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);

	void GetMethodsFromDocumentPosition([In] ISymUnmanagedDocument document, [In] uint line, [In] uint column, [In] uint cMethod, out uint pcMethod, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ISymUnmanagedMethod[] pRetVal);

	void GetDocumentVersion([In] ISymUnmanagedDocument pDoc, out int version, out bool pbCurrent);

	void GetMethodVersion([In] ISymUnmanagedMethod pMethod, out int version);
}
