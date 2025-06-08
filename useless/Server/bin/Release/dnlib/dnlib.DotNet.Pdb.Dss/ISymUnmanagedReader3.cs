using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("6151CAD9-E1EE-437A-A808-F64838C0D046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedReader3 : ISymUnmanagedReader2, ISymUnmanagedReader
{
	void _VtblGap1_20();

	void GetSymAttributeByVersion(uint token, uint version, [MarshalAs(UnmanagedType.LPWStr)] string name, uint cBuffer, out uint pcBuffer, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buffer);

	void GetSymAttributeByVersionPreRemap(int methodToken, int version, [MarshalAs(UnmanagedType.LPWStr)] string name, int cBuffer, out int pcBuffer, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buffer);
}
