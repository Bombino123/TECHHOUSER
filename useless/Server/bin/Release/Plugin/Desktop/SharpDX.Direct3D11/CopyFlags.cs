using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum CopyFlags
{
	NoOverwrite = 1,
	Discard = 2,
	None = 0
}
