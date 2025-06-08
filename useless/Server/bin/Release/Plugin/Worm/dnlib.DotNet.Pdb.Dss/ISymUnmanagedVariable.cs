using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("9F60EEBE-2D9A-3F7C-BF58-80BC991C60BB")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedVariable
{
	void GetName([In] uint cchName, out uint pcchName, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);

	void GetAttributes(out uint pRetVal);

	void GetSignature([In] uint cSig, out uint pcSig, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] sig);

	void GetAddressKind(out uint pRetVal);

	void GetAddressField1(out uint pRetVal);

	void GetAddressField2(out uint pRetVal);

	void GetAddressField3(out uint pRetVal);

	void GetStartOffset(out uint pRetVal);

	void GetEndOffset(out uint pRetVal);
}
