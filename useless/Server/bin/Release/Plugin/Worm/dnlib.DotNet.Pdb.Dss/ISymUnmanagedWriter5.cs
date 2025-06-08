using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("DCF7780D-BDE9-45DF-ACFE-21731A32000C")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedWriter5 : ISymUnmanagedWriter4, ISymUnmanagedWriter3, ISymUnmanagedWriter2
{
	void _VtblGap1_30();

	void OpenMapTokensToSourceSpans();

	void CloseMapTokensToSourceSpans();

	void MapTokenToSourceSpan(uint token, ISymUnmanagedDocumentWriter document, uint line, uint column, uint endLine, uint endColumn);
}
