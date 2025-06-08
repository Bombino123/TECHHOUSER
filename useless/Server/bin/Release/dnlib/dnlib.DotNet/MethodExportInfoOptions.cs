using System;

namespace dnlib.DotNet;

[Flags]
public enum MethodExportInfoOptions
{
	None = 0,
	FromUnmanaged = 1,
	FromUnmanagedRetainAppDomain = 2,
	CallMostDerived = 4
}
