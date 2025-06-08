using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum DebugFeatureFlags
{
	FlushPerRender = 1,
	FinishPerRender = 2,
	PresentPerRender = 4,
	AlwaysDiscardOfferedResource = 8,
	NeverDiscardOfferedResource = 0x10,
	AvoidBehaviorChangingDebugAids = 0x40,
	DisableTiledResourceMappingTrackingAndValidation = 0x80
}
