using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("0B97726E-9E6D-4F05-9A26-424022093CAA")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedWriter2
{
	void DefineDocument([In][MarshalAs(UnmanagedType.LPWStr)] string url, [In] ref Guid language, [In] ref Guid languageVendor, [In] ref Guid documentType, out ISymUnmanagedDocumentWriter pRetVal);

	void SetUserEntryPoint([In] uint entryMethod);

	void OpenMethod([In] uint method);

	void CloseMethod();

	void OpenScope([In] uint startOffset, out uint pRetVal);

	void CloseScope([In] uint endOffset);

	void SetScopeRange([In] uint scopeID, [In] uint startOffset, [In] uint endOffset);

	void DefineLocalVariable([In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] uint attributes, [In] uint cSig, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] signature, [In] uint addrKind, [In] uint addr1, [In] uint addr2, [In] uint addr3, [In] uint startOffset, [In] uint endOffset);

	void DefineParameter([In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] uint attributes, [In] uint sequence, [In] uint addrKind, [In] uint addr1, [In] uint addr2, [In] uint addr3);

	void DefineField([In] uint parent, [In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] uint attributes, [In] uint cSig, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] signature, [In] uint addrKind, [In] uint addr1, [In] uint addr2, [In] uint addr3);

	void DefineGlobalVariable([In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] uint attributes, [In] uint cSig, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] signature, [In] uint addrKind, [In] uint addr1, [In] uint addr2, [In] uint addr3);

	void Close();

	void SetSymAttribute([In] uint parent, [In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] uint cData, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data);

	void OpenNamespace([In][MarshalAs(UnmanagedType.LPWStr)] string name);

	void CloseNamespace();

	void UsingNamespace([In][MarshalAs(UnmanagedType.LPWStr)] string fullName);

	void SetMethodSourceRange([In] ISymUnmanagedDocumentWriter startDoc, [In] uint startLine, [In] uint startColumn, [In] ISymUnmanagedDocumentWriter endDoc, [In] uint endLine, [In] uint endColumn);

	void Initialize([In][MarshalAs(UnmanagedType.IUnknown)] object emitter, [In][MarshalAs(UnmanagedType.LPWStr)] string filename, [In] IStream pIStream, [In] bool fFullBuild);

	void GetDebugInfo(out IMAGE_DEBUG_DIRECTORY pIDD, [In] uint cData, out uint pcData, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data);

	void DefineSequencePoints([In] ISymUnmanagedDocumentWriter document, [In] uint spCount, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] offsets, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] lines, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] columns, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] endLines, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] endColumns);

	void RemapToken([In] uint oldToken, [In] uint newToken);

	void Initialize2([In][MarshalAs(UnmanagedType.IUnknown)] object emitter, [In][MarshalAs(UnmanagedType.LPWStr)] string tempfilename, [In] IStream pIStream, [In] bool fFullBuild, [In][MarshalAs(UnmanagedType.LPWStr)] string finalfilename);

	void DefineConstant([In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] object value, [In] uint cSig, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] signature);

	void Abort();

	void DefineLocalVariable2([In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] uint attributes, [In] uint sigToken, [In] uint addrKind, [In] uint addr1, [In] uint addr2, [In] uint addr3, [In] uint startOffset, [In] uint endOffset);

	void DefineGlobalVariable2([In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] uint attributes, [In] uint sigToken, [In] uint addrKind, [In] uint addr1, [In] uint addr2, [In] uint addr3);

	void DefineConstant2([In][MarshalAs(UnmanagedType.LPWStr)] string name, [In] object value, [In] uint sigToken);
}
