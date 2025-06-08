using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("5DA320C8-9C2C-4E5A-B823-027E0677B359")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedMethod2 : ISymUnmanagedMethod
{
	void _VtblGap1_10();

	void GetLocalSignatureToken(out uint token);
}
