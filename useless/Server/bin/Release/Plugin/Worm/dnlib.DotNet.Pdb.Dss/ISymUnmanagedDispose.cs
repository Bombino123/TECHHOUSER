using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("969708D2-05E5-4861-A3B0-96E473CDF63F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedDispose
{
	[PreserveSig]
	int Destroy();
}
