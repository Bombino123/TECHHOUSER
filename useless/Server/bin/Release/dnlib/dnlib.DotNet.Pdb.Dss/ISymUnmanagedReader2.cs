using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("A09E53B2-2A57-4cca-8F63-B84F7C35D4AA")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedReader2 : ISymUnmanagedReader
{
	void _VtblGap1_17();

	void GetMethodByVersionPreRemap(uint token, uint version, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedMethod pRetVal);

	void GetSymAttributePreRemap(uint parent, [In][MarshalAs(UnmanagedType.LPWStr)] string name, uint cBuffer, out uint pcBuffer, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer);

	void GetMethodsInDocument(ISymUnmanagedDocument document, uint bufferLength, out uint count, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ISymUnmanagedMethod[] methods);
}
