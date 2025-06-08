using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum UnorderedAccessViewBufferFlags
{
	Raw = 1,
	Append = 2,
	Counter = 4,
	None = 0
}
