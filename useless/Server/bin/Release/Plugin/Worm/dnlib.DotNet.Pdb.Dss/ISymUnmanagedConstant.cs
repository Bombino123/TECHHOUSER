using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("48B25ED8-5BAD-41bc-9CEE-CD62FABC74E9")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedConstant
{
	void GetName([In] uint cchName, out uint pcchName, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);

	void GetValue(out object pValue);

	[PreserveSig]
	int GetSignature([In] uint cSig, out uint pcSig, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] sig);
}
