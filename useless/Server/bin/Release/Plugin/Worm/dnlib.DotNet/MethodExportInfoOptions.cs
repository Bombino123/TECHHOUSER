using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum MethodExportInfoOptions
{
	None = 0,
	FromUnmanaged = 1,
	FromUnmanagedRetainAppDomain = 2,
	CallMostDerived = 4
}
