using System;
using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("651B9AD0-0FC7-4AA9-9538-D89931010741")]
internal interface IMediaObjectInPlace
{
	[PreserveSig]
	int Process([In] int size, [In] IntPtr data, [In] long refTimeStart, [In] DmoInPlaceProcessFlags dwFlags);

	[PreserveSig]
	int Clone([MarshalAs(UnmanagedType.Interface)] out IMediaObjectInPlace mediaObjectInPlace);

	[PreserveSig]
	int GetLatency(out long latencyTime);
}
