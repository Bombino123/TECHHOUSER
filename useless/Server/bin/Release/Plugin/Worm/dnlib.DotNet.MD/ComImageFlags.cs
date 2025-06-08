using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[Flags]
[ComVisible(true)]
public enum ComImageFlags : uint
{
	ILOnly = 1u,
	Bit32Required = 2u,
	ILLibrary = 4u,
	StrongNameSigned = 8u,
	NativeEntryPoint = 0x10u,
	TrackDebugData = 0x10000u,
	Bit32Preferred = 0x20000u
}
