using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("12F1E02C-1E05-4B0E-9468-EBC9D1BB040F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedWriter3 : ISymUnmanagedWriter2
{
	void _VtblGap1_27();

	void OpenMethod2(uint method, uint isect, uint offset);

	void Commit();
}
