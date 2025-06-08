using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum DeviceCreationFlags
{
	SingleThreaded = 1,
	Debug = 2,
	SwitchToRef = 4,
	PreventThreadingOptimizations = 8,
	BgraSupport = 0x20,
	Debuggable = 0x40,
	PreventAlteringLayerSettingsFromRegistry = 0x80,
	DisableGpuTimeout = 0x100,
	VideoSupport = 0x800,
	None = 0
}
