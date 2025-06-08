using System;

namespace SharpDX.DXGI;

[Flags]
public enum DisplayModeEnumerationFlags
{
	Interlaced = 1,
	Scaling = 2,
	Stereo = 4,
	DisabledStereo = 8
}
