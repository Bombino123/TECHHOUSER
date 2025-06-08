using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedDocumentWriter
{
	void SetSource([In] uint sourceSize, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] source);

	void SetCheckSum([In] Guid algorithmId, [In] uint checkSumSize, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] checkSum);
}
