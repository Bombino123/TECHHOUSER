using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum AssemblyAttributes : uint
{
	None = 0u,
	PublicKey = 1u,
	PA_None = 0u,
	PA_MSIL = 0x10u,
	PA_x86 = 0x20u,
	PA_IA64 = 0x30u,
	PA_AMD64 = 0x40u,
	PA_ARM = 0x50u,
	PA_ARM64 = 0x60u,
	PA_NoPlatform = 0x70u,
	PA_Specified = 0x80u,
	PA_Mask = 0x70u,
	PA_FullMask = 0xF0u,
	PA_Shift = 4u,
	EnableJITcompileTracking = 0x8000u,
	DisableJITcompileOptimizer = 0x4000u,
	Retargetable = 0x100u,
	ContentType_Default = 0u,
	ContentType_WindowsRuntime = 0x200u,
	ContentType_Mask = 0xE00u
}
