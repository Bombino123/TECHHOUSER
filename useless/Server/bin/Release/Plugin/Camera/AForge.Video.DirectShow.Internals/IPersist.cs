using System;
using System.Runtime.InteropServices;

namespace AForge.Video.DirectShow.Internals;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
[Guid("0000010c-0000-0000-C000-000000000046")]
internal interface IPersist
{
	[PreserveSig]
	int GetClassID(out Guid pClassID);
}
