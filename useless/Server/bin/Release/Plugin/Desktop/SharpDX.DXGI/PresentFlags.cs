using System;

namespace SharpDX.DXGI;

[Flags]
public enum PresentFlags
{
	Test = 1,
	DoNotSequence = 2,
	Restart = 4,
	DoNotWait = 8,
	StereoPreferRight = 0x10,
	StereoTemporaryMono = 0x20,
	RestrictToOutput = 0x40,
	UseDuration = 0x100,
	AllowTearing = 0x200,
	None = 0
}
