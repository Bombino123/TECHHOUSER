using System;

namespace SharpDX.DXGI;

[Flags]
public enum SwapChainFlags
{
	Nonprerotated = 1,
	AllowModeSwitch = 2,
	GdiCompatible = 4,
	RestrictedContent = 8,
	RestrictSharedResourceDriver = 0x10,
	DisplayOnly = 0x20,
	FrameLatencyWaitAbleObject = 0x40,
	ForegroundLayer = 0x80,
	FullScreenVideo = 0x100,
	YuvVideo = 0x200,
	HwProtected = 0x400,
	AllowTearing = 0x800,
	RestrictedToAllHolographicDisplayS = 0x1000,
	None = 0
}
