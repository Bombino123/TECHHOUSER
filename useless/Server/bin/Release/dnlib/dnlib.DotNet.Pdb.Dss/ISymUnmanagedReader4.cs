using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("E65C58B7-2948-434D-8A6D-481740A00C16")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedReader4 : ISymUnmanagedReader3, ISymUnmanagedReader2, ISymUnmanagedReader
{
	void _VtblGap1_22();

	[PreserveSig]
	int MatchesModule(Guid guid, uint stamp, uint age, [MarshalAs(UnmanagedType.Bool)] out bool result);

	void GetPortableDebugMetadata(out IntPtr pMetadata, out uint pcMetadata);

	[PreserveSig]
	int GetSourceServerData(out IntPtr data, out int pcData);
}
