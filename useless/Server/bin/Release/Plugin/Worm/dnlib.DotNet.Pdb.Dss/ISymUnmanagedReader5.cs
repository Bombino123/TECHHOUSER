using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("6576C987-7E8D-4298-A6E1-6F9783165F07")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedReader5 : ISymUnmanagedReader4, ISymUnmanagedReader3, ISymUnmanagedReader2, ISymUnmanagedReader
{
	void _VtblGap1_25();

	void GetPortableDebugMetadataByVersion(uint version, out IntPtr pMetadata, out uint pcMetadata);
}
