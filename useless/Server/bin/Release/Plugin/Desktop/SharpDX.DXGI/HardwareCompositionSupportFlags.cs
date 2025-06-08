using System;

namespace SharpDX.DXGI;

[Flags]
public enum HardwareCompositionSupportFlags
{
	FullScreen = 1,
	Windowed = 2,
	CursorStretched = 4,
	None = 0
}
